using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.OrderModule
{
    public class OrderRepository : BaseRepository, IOrderRepository
    {
        private readonly IOrderItemRepository _itemRepository;

        public OrderRepository(MyCon dbConnection) : base(dbConnection)
        {
            _itemRepository = new OrderItemRepository(dbConnection);
        }

        public OrderRepository(MyCon dbConnection, IOrderItemRepository itemRepository) : base(dbConnection)
        {
            _itemRepository = itemRepository;
        }

        // ---------------------------------------------------------------
        //  Private mapper
        // ---------------------------------------------------------------

        private Order MapReaderToOrder(DbDataReader reader)
        {
            try
            {
                return new Order
                {
                    OrderId         = ReadValue<int>(reader,      "order_id",         0),
                    UserId          = ReadValue<int>(reader,      "user_id",          0),
                    OrderDate       = ReadValue<DateTime>(reader, "order_date",       DateTime.UtcNow),
                    TotalAmount     = ReadValue<double>(reader,   "total_amount",     0.0),
                    PaymentMethod   = ReadValue<string?>(reader,  "payment_method",   null),
                    DeliveryAddress = ReadValue<string?>(reader,  "delivery_address", null),
                    OrderStatus     = ReadValue<string>(reader,   "order_status",     "Pending"),
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Failed to map database row to Order. Check schema/type alignment.", ex);
            }
        }

        // ---------------------------------------------------------------
        //  Read operations
        // ---------------------------------------------------------------

        /// <summary>Returns all orders ordered by order_date DESC.</summary>
        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            var orders = await ExecuteReaderToListAsync(
                "SELECT * FROM \"orders\" ORDER BY \"order_date\" DESC",
                MapReaderToOrder);

            foreach (var order in orders)
            {
                order.Items = (await _itemRepository.GetByOrderIdAsync(order.OrderId)).ToList();
            }

            return orders;
        }

        /// <summary>Returns a single order by primary key.</summary>
        public async Task<Order?> GetByIdAsync(int orderId, bool includeItems = true)
        {
            var results = await ExecuteReaderToListAsync(
                "SELECT * FROM \"orders\" WHERE \"order_id\" = @order_id",
                MapReaderToOrder,
                new[] { CreateParameter("order_id", orderId) });

            var order = results.FirstOrDefault();
            if (order != null && includeItems)
            {
                order.Items = (await _itemRepository.GetByOrderIdAsync(order.OrderId)).ToList();
            }

            return order;
        }

        /// <summary>Returns all orders for a specific user.</summary>
        public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId, bool includeItems = true)
        {
            var orders = await ExecuteReaderToListAsync(
                "SELECT * FROM \"orders\" WHERE \"user_id\" = @user_id ORDER BY \"order_date\" DESC",
                MapReaderToOrder,
                new[] { CreateParameter("user_id", userId) });

            if (includeItems)
            {
                foreach (var order in orders)
                {
                    order.Items = (await _itemRepository.GetByOrderIdAsync(order.OrderId)).ToList();
                }
            }

            return orders;
        }

        // ---------------------------------------------------------------
        //  Write / Order placement operations
        // ---------------------------------------------------------------

        /// <summary>Creates a new order with items inside a database transaction.</summary>
        public async Task<Order> CreateOrderAsync(int userId, string? paymentMethod, string? deliveryAddress, IEnumerable<OrderItem> items)
        {
            var itemList = items?.ToList() ?? new List<OrderItem>();
            if (itemList.Count == 0)
                throw new ArgumentException("Cannot create an order without items.", nameof(items));

            double totalAmount = itemList.Sum(i => i.Subtotal > 0 ? i.Subtotal : (i.UnitPrice * i.Quantity));

            return await WithTransactionAsync(async (conn, tx) =>
            {
                const string insertOrderSql =
                    "INSERT INTO \"orders\" (" +
                    "\"user_id\", \"total_amount\", \"payment_method\", \"delivery_address\", \"order_status\"" +
                    ") VALUES (" +
                    "@user_id, @total_amount, @payment_method, @delivery_address, 'Pending'" +
                    "); SELECT last_insert_rowid();";

                var cmd = _db.CreateCommand(conn, insertOrderSql);
                cmd.Transaction = tx;
                cmd.Parameters.Add(CreateParameter("user_id", userId));
                cmd.Parameters.Add(CreateParameter("total_amount", totalAmount));
                cmd.Parameters.Add(CreateParameter("payment_method", (object?)paymentMethod ?? DBNull.Value));
                cmd.Parameters.Add(CreateParameter("delivery_address", (object?)deliveryAddress ?? DBNull.Value));

                var orderIdObj = await cmd.ExecuteScalarAsync();
                int orderId = Convert.ToInt32(orderIdObj);

                foreach (var item in itemList)
                {
                    item.OrderId = orderId;
                    if (item.Subtotal <= 0) item.Subtotal = item.UnitPrice * item.Quantity;

                    const string insertItemSql =
                        "INSERT INTO \"order_items\" (" +
                        "\"order_id\", \"medicine_id\", \"quantity\", \"unit_price\", \"subtotal\"" +
                        ") VALUES (" +
                        "@order_id, @medicine_id, @quantity, @unit_price, @subtotal" +
                        ")";

                    var itemCmd = _db.CreateCommand(conn, insertItemSql);
                    itemCmd.Transaction = tx;
                    itemCmd.Parameters.Add(CreateParameter("order_id", orderId));
                    itemCmd.Parameters.Add(CreateParameter("medicine_id", item.MedicineId));
                    itemCmd.Parameters.Add(CreateParameter("quantity", item.Quantity));
                    itemCmd.Parameters.Add(CreateParameter("unit_price", item.UnitPrice));
                    itemCmd.Parameters.Add(CreateParameter("subtotal", item.Subtotal));
                    await itemCmd.ExecuteNonQueryAsync();

                    // Deduct stock quantity in medicines table
                    const string updateStockSql =
                        "UPDATE \"medicines\" SET \"stock_qty\" = MAX(0, \"stock_qty\" - @quantity) WHERE \"medicine_id\" = @medicine_id";
                    var stockCmd = _db.CreateCommand(conn, updateStockSql);
                    stockCmd.Transaction = tx;
                    stockCmd.Parameters.Add(CreateParameter("quantity", item.Quantity));
                    stockCmd.Parameters.Add(CreateParameter("medicine_id", item.MedicineId));
                    await stockCmd.ExecuteNonQueryAsync();
                }

                return new Order
                {
                    OrderId = orderId,
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = totalAmount,
                    PaymentMethod = paymentMethod,
                    DeliveryAddress = deliveryAddress,
                    OrderStatus = "Pending",
                    Items = itemList
                };
            });
        }

        /// <summary>Checkout directly from user's active cart into an order and clear cart atomically.</summary>
        public async Task<Order> CheckoutFromCartAsync(int userId, string? paymentMethod, string? deliveryAddress)
        {
            return await WithTransactionAsync(async (conn, tx) =>
            {
                // 1. Fetch user's cart items joining medicines to get current unit_price
                const string getCartSql =
                    "SELECT c.\"cart_id\", c.\"medicine_id\", c.\"quantity\", c.\"subtotal\", m.\"price\" " +
                    "FROM \"cart\" c " +
                    "JOIN \"medicines\" m ON c.\"medicine_id\" = m.\"medicine_id\" " +
                    "WHERE c.\"user_id\" = @user_id";

                var cartCmd = _db.CreateCommand(conn, getCartSql);
                cartCmd.Transaction = tx;
                cartCmd.Parameters.Add(CreateParameter("user_id", userId));

                var orderItems = new List<OrderItem>();
                using (var reader = await cartCmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int medicineId = Convert.ToInt32(reader["medicine_id"]);
                        int qty = Convert.ToInt32(reader["quantity"]);
                        double unitPrice = Convert.ToDouble(reader["price"]);
                        double subtotal = Convert.ToDouble(reader["subtotal"]);
                        if (subtotal <= 0) subtotal = unitPrice * qty;

                        orderItems.Add(new OrderItem
                        {
                            MedicineId = medicineId,
                            Quantity = qty,
                            UnitPrice = unitPrice,
                            Subtotal = subtotal
                        });
                    }
                }

                if (orderItems.Count == 0)
                    throw new InvalidOperationException("Cannot checkout: Cart is empty.");

                double totalAmount = orderItems.Sum(i => i.Subtotal);

                // 2. Insert order
                const string insertOrderSql =
                    "INSERT INTO \"orders\" (" +
                    "\"user_id\", \"total_amount\", \"payment_method\", \"delivery_address\", \"order_status\"" +
                    ") VALUES (" +
                    "@user_id, @total_amount, @payment_method, @delivery_address, 'Pending'" +
                    "); SELECT last_insert_rowid();";

                var orderCmd = _db.CreateCommand(conn, insertOrderSql);
                orderCmd.Transaction = tx;
                orderCmd.Parameters.Add(CreateParameter("user_id", userId));
                orderCmd.Parameters.Add(CreateParameter("total_amount", totalAmount));
                orderCmd.Parameters.Add(CreateParameter("payment_method", (object?)paymentMethod ?? DBNull.Value));
                orderCmd.Parameters.Add(CreateParameter("delivery_address", (object?)deliveryAddress ?? DBNull.Value));

                int orderId = Convert.ToInt32(await orderCmd.ExecuteScalarAsync());

                // 3. Insert order items & deduct medicine stock
                foreach (var item in orderItems)
                {
                    item.OrderId = orderId;

                    const string insertItemSql =
                        "INSERT INTO \"order_items\" (" +
                        "\"order_id\", \"medicine_id\", \"quantity\", \"unit_price\", \"subtotal\"" +
                        ") VALUES (" +
                        "@order_id, @medicine_id, @quantity, @unit_price, @subtotal" +
                        ")";

                    var itemCmd = _db.CreateCommand(conn, insertItemSql);
                    itemCmd.Transaction = tx;
                    itemCmd.Parameters.Add(CreateParameter("order_id", orderId));
                    itemCmd.Parameters.Add(CreateParameter("medicine_id", item.MedicineId));
                    itemCmd.Parameters.Add(CreateParameter("quantity", item.Quantity));
                    itemCmd.Parameters.Add(CreateParameter("unit_price", item.UnitPrice));
                    itemCmd.Parameters.Add(CreateParameter("subtotal", item.Subtotal));
                    await itemCmd.ExecuteNonQueryAsync();

                    const string updateStockSql =
                        "UPDATE \"medicines\" SET \"stock_qty\" = MAX(0, \"stock_qty\" - @quantity) WHERE \"medicine_id\" = @medicine_id";
                    var stockCmd = _db.CreateCommand(conn, updateStockSql);
                    stockCmd.Transaction = tx;
                    stockCmd.Parameters.Add(CreateParameter("quantity", item.Quantity));
                    stockCmd.Parameters.Add(CreateParameter("medicine_id", item.MedicineId));
                    await stockCmd.ExecuteNonQueryAsync();
                }

                // 4. Clear user's cart
                const string clearCartSql = "DELETE FROM \"cart\" WHERE \"user_id\" = @user_id";
                var clearCmd = _db.CreateCommand(conn, clearCartSql);
                clearCmd.Transaction = tx;
                clearCmd.Parameters.Add(CreateParameter("user_id", userId));
                await clearCmd.ExecuteNonQueryAsync();

                return new Order
                {
                    OrderId = orderId,
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = totalAmount,
                    PaymentMethod = paymentMethod,
                    DeliveryAddress = deliveryAddress,
                    OrderStatus = "Pending",
                    Items = orderItems
                };
            });
        }

        public async Task AddAsync(Order entity)
        {
            const string sql =
                "INSERT INTO \"orders\" (" +
                "\"user_id\", \"total_amount\", \"payment_method\", \"delivery_address\", \"order_status\"" +
                ") VALUES (" +
                "@user_id, @total_amount, @payment_method, @delivery_address, @order_status" +
                ")";

            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("user_id",          entity.UserId),
                CreateParameter("total_amount",     entity.TotalAmount),
                CreateParameter("payment_method",   (object?)entity.PaymentMethod   ?? DBNull.Value),
                CreateParameter("delivery_address", (object?)entity.DeliveryAddress ?? DBNull.Value),
                CreateParameter("order_status",     string.IsNullOrWhiteSpace(entity.OrderStatus) ? "Pending" : entity.OrderStatus),
            });
        }

        public async Task UpdateAsync(Order entity)
        {
            const string sql =
                "UPDATE \"orders\" SET " +
                "\"user_id\" = @user_id, " +
                "\"total_amount\" = @total_amount, " +
                "\"payment_method\" = @payment_method, " +
                "\"delivery_address\" = @delivery_address, " +
                "\"order_status\" = @order_status " +
                "WHERE \"order_id\" = @order_id";

            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("order_id",         entity.OrderId),
                CreateParameter("user_id",          entity.UserId),
                CreateParameter("total_amount",     entity.TotalAmount),
                CreateParameter("payment_method",   (object?)entity.PaymentMethod   ?? DBNull.Value),
                CreateParameter("delivery_address", (object?)entity.DeliveryAddress ?? DBNull.Value),
                CreateParameter("order_status",     string.IsNullOrWhiteSpace(entity.OrderStatus) ? "Pending" : entity.OrderStatus),
            });
        }

        public async Task UpdateStatusAsync(int orderId, string newStatus)
        {
            const string sql = "UPDATE \"orders\" SET \"order_status\" = @order_status WHERE \"order_id\" = @order_id";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("order_id",     orderId),
                CreateParameter("order_status", newStatus),
            });
        }

        public async Task DeleteAsync(int orderId)
        {
            await _itemRepository.DeleteByOrderIdAsync(orderId);
            await ExecuteNonQueryAsync(
                "DELETE FROM \"orders\" WHERE \"order_id\" = @order_id",
                new[] { CreateParameter("order_id", orderId) });
        }

        public async Task DeleteAllAsync()
        {
            await ExecuteNonQueryAsync("DELETE FROM \"order_items\"");
            await ExecuteNonQueryAsync("DELETE FROM \"orders\"");
        }

        public async Task<PaginationModel<Order>> GetPaginatedAsync(int pageNumber, int pageSize, int? userId = null, string? status = null)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize < 0)    pageSize   = 0;

            var whereClauses = new List<string>();
            var parameters = new List<DbParameter>();

            if (userId.HasValue && userId.Value > 0)
            {
                whereClauses.Add("\"user_id\" = @userId");
                parameters.Add(CreateParameter("userId", userId.Value));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                whereClauses.Add("\"order_status\" = @status");
                parameters.Add(CreateParameter("status", status));
            }

            string whereSql = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";

            if (pageSize == 0)
            {
                string fullSql = $"SELECT * FROM \"orders\" {whereSql} ORDER BY \"order_date\" DESC";
                var all = await ExecuteReaderToListAsync(fullSql, MapReaderToOrder, parameters);
                foreach (var order in all)
                {
                    order.Items = (await _itemRepository.GetByOrderIdAsync(order.OrderId)).ToList();
                }
                int total = all.Count;
                return new PaginationModel<Order>
                {
                    Items       = all,
                    TotalCount  = total,
                    PageSize    = total,
                    CurrentPage = 1,
                };
            }

            string dataSql =
                $"SELECT * FROM \"orders\" {whereSql} ORDER BY \"order_date\" DESC " +
                "LIMIT @pageSize OFFSET @offset";

            var dataParameters = new List<DbParameter>(parameters)
            {
                CreateParameter("offset", (pageNumber - 1) * pageSize),
                CreateParameter("pageSize", pageSize)
            };

            var items = await ExecuteReaderToListAsync(dataSql, MapReaderToOrder, dataParameters);
            foreach (var order in items)
            {
                order.Items = (await _itemRepository.GetByOrderIdAsync(order.OrderId)).ToList();
            }

            string countSql = $"SELECT COUNT(*) FROM \"orders\" {whereSql}";
            var totalCount = await ExecuteScalarAsync<int>(countSql, parameters);

            return new PaginationModel<Order>
            {
                Items       = items,
                TotalCount  = totalCount,
                PageSize    = pageSize,
                CurrentPage = pageNumber,
            };
        }
    }
}

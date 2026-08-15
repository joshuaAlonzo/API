using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.OrderModule
{
    public class OrderItemRepository : BaseRepository, IOrderItemRepository
    {
        public OrderItemRepository(MyCon dbConnection) : base(dbConnection) { }

        private OrderItem MapReaderToOrderItem(DbDataReader reader)
        {
            try
            {
                return new OrderItem
                {
                    OrderItemId = ReadValue<int>(reader,    "order_item_id", 0),
                    OrderId     = ReadValue<int>(reader,    "order_id",      0),
                    MedicineId  = ReadValue<int>(reader,    "medicine_id",   0),
                    Quantity    = ReadValue<int>(reader,    "quantity",      0),
                    UnitPrice   = ReadValue<double>(reader, "unit_price",    0.0),
                    Subtotal    = ReadValue<double>(reader, "subtotal",      0.0),
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Failed to map database row to OrderItem. Check schema/type alignment.", ex);
            }
        }

        public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId)
        {
            return await ExecuteReaderToListAsync(
                "SELECT * FROM \"order_items\" WHERE \"order_id\" = @order_id ORDER BY \"order_item_id\" ASC",
                MapReaderToOrderItem,
                new[] { CreateParameter("order_id", orderId) });
        }

        public async Task<OrderItem?> GetByIdAsync(int orderItemId)
        {
            var results = await ExecuteReaderToListAsync(
                "SELECT * FROM \"order_items\" WHERE \"order_item_id\" = @order_item_id",
                MapReaderToOrderItem,
                new[] { CreateParameter("order_item_id", orderItemId) });
            return results.FirstOrDefault();
        }

        public async Task AddAsync(OrderItem entity)
        {
            const string sql =
                "INSERT INTO \"order_items\" (" +
                "\"order_id\", \"medicine_id\", \"quantity\", \"unit_price\", \"subtotal\"" +
                ") VALUES (" +
                "@order_id, @medicine_id, @quantity, @unit_price, @subtotal" +
                ")";

            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("order_id",    entity.OrderId),
                CreateParameter("medicine_id", entity.MedicineId),
                CreateParameter("quantity",    entity.Quantity),
                CreateParameter("unit_price",  entity.UnitPrice),
                CreateParameter("subtotal",    entity.Subtotal),
            });
        }

        public async Task UpdateAsync(OrderItem entity)
        {
            const string sql =
                "UPDATE \"order_items\" SET " +
                "\"order_id\" = @order_id, " +
                "\"medicine_id\" = @medicine_id, " +
                "\"quantity\" = @quantity, " +
                "\"unit_price\" = @unit_price, " +
                "\"subtotal\" = @subtotal " +
                "WHERE \"order_item_id\" = @order_item_id";

            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("order_item_id", entity.OrderItemId),
                CreateParameter("order_id",      entity.OrderId),
                CreateParameter("medicine_id",   entity.MedicineId),
                CreateParameter("quantity",      entity.Quantity),
                CreateParameter("unit_price",    entity.UnitPrice),
                CreateParameter("subtotal",      entity.Subtotal),
            });
        }

        public async Task DeleteAsync(int orderItemId)
        {
            await ExecuteNonQueryAsync(
                "DELETE FROM \"order_items\" WHERE \"order_item_id\" = @order_item_id",
                new[] { CreateParameter("order_item_id", orderItemId) });
        }

        public async Task DeleteByOrderIdAsync(int orderId)
        {
            await ExecuteNonQueryAsync(
                "DELETE FROM \"order_items\" WHERE \"order_id\" = @order_id",
                new[] { CreateParameter("order_id", orderId) });
        }
    }
}

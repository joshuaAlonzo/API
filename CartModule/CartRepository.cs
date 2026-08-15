using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.CartModule
{
    public class CartRepository : BaseRepository, ICartRepository
    {
        public CartRepository(MyCon dbConnection) : base(dbConnection) { }

        // ---------------------------------------------------------------
        //  Private mapper
        // ---------------------------------------------------------------

        private Cart MapReaderToCart(DbDataReader reader)
        {
            try
            {
                return new Cart
                {
                    CartId     = ReadValue<int>(reader,      "cart_id",     0),
                    UserId     = ReadValue<int>(reader,      "user_id",     0),
                    MedicineId = ReadValue<int>(reader,      "medicine_id", 0),
                    Quantity   = ReadValue<int>(reader,      "quantity",    0),
                    Subtotal   = ReadValue<double>(reader,   "subtotal",    0.0),
                    AddedAt    = ReadValue<DateTime>(reader, "added_at",    DateTime.MinValue),
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Failed to map database row to Cart. Check schema/type alignment.", ex);
            }
        }

        // ---------------------------------------------------------------
        //  Read operations
        // ---------------------------------------------------------------

        /// <summary>Returns all rows from the cart table, newest first.</summary>
        public async Task<IEnumerable<Cart>> GetAllAsync()
        {
            return await ExecuteReaderToListAsync(
                "SELECT * FROM \"cart\" ORDER BY \"added_at\" DESC",
                MapReaderToCart);
        }

        /// <summary>Returns a single cart item by its primary key.</summary>
        public async Task<Cart?> GetByIdAsync(int cartId)
        {
            var results = await ExecuteReaderToListAsync(
                "SELECT * FROM \"cart\" WHERE \"cart_id\" = @cart_id",
                MapReaderToCart,
                new[] { CreateParameter("cart_id", cartId) });
            return results.FirstOrDefault();
        }

        /// <summary>Returns all items in a user's cart, newest first.</summary>
        public async Task<IEnumerable<Cart>> GetByUserIdAsync(int userId)
        {
            return await ExecuteReaderToListAsync(
                "SELECT * FROM \"cart\" WHERE \"user_id\" = @user_id ORDER BY \"added_at\" DESC",
                MapReaderToCart,
                new[] { CreateParameter("user_id", userId) });
        }

        /// <summary>Returns the cart row for a specific user + medicine pair, or null.</summary>
        public async Task<Cart?> GetByUserAndMedicineAsync(int userId, int medicineId)
        {
            var results = await ExecuteReaderToListAsync(
                "SELECT * FROM \"cart\" WHERE \"user_id\" = @user_id AND \"medicine_id\" = @medicine_id",
                MapReaderToCart,
                new[]
                {
                    CreateParameter("user_id",     userId),
                    CreateParameter("medicine_id", medicineId),
                });
            return results.FirstOrDefault();
        }

        // ---------------------------------------------------------------
        //  Write operations
        // ---------------------------------------------------------------

        /// <summary>Inserts a new cart item row.</summary>
        public async Task AddAsync(Cart entity)
        {
            const string sql =
                "INSERT INTO \"cart\" (\"user_id\", \"medicine_id\", \"quantity\", \"subtotal\", \"added_at\") " +
                "VALUES (@user_id, @medicine_id, @quantity, @subtotal, @added_at)";

            var parameters = new[]
            {
                CreateParameter("user_id",     entity.UserId),
                CreateParameter("medicine_id", entity.MedicineId),
                CreateParameter("quantity",    entity.Quantity),
                CreateParameter("subtotal",    entity.Subtotal),
                CreateParameter("added_at",    entity.AddedAt == DateTime.MinValue
                                                   ? (object)DateTime.UtcNow
                                                   : entity.AddedAt),
            };

            await ExecuteNonQueryAsync(sql, parameters);
        }

        /// <summary>Updates quantity and subtotal for an existing cart item.</summary>
        public async Task UpdateAsync(Cart entity)
        {
            const string sql =
                "UPDATE \"cart\" SET " +
                "\"user_id\" = @user_id, " +
                "\"medicine_id\" = @medicine_id, " +
                "\"quantity\" = @quantity, " +
                "\"subtotal\" = @subtotal " +
                "WHERE \"cart_id\" = @cart_id";

            var parameters = new[]
            {
                CreateParameter("cart_id",     entity.CartId),
                CreateParameter("user_id",     entity.UserId),
                CreateParameter("medicine_id", entity.MedicineId),
                CreateParameter("quantity",    entity.Quantity),
                CreateParameter("subtotal",    entity.Subtotal),
            };

            await ExecuteNonQueryAsync(sql, parameters);
        }

        // ---------------------------------------------------------------
        //  Delete operations
        // ---------------------------------------------------------------

        /// <summary>Deletes a single cart row by primary key.</summary>
        public async Task DeleteAsync(int cartId)
        {
            await ExecuteNonQueryAsync(
                "DELETE FROM \"cart\" WHERE \"cart_id\" = @cart_id",
                new[] { CreateParameter("cart_id", cartId) });
        }

        /// <summary>Removes a specific medicine from a user's cart.</summary>
        public async Task DeleteByUserAndMedicineAsync(int userId, int medicineId)
        {
            await ExecuteNonQueryAsync(
                "DELETE FROM \"cart\" WHERE \"user_id\" = @user_id AND \"medicine_id\" = @medicine_id",
                new[]
                {
                    CreateParameter("user_id",     userId),
                    CreateParameter("medicine_id", medicineId),
                });
        }

        /// <summary>Removes all items from a user's cart.</summary>
        public async Task ClearCartAsync(int userId)
        {
            await ExecuteNonQueryAsync(
                "DELETE FROM \"cart\" WHERE \"user_id\" = @user_id",
                new[] { CreateParameter("user_id", userId) });
        }

        /// <summary>Deletes every row in the cart table.</summary>
        public async Task DeleteAllAsync()
        {
            await ExecuteNonQueryAsync("DELETE FROM \"cart\"");
        }

        // ---------------------------------------------------------------
        //  Filtered / paginated queries
        // ---------------------------------------------------------------

        /// <summary>
        /// Exact-match filter. Supported keys: cart_id, user_id, medicine_id.
        /// Returns all rows when no filters are provided.
        /// </summary>
        public async Task<IEnumerable<Cart>> GetFilteredExactAsync(
            IReadOnlyDictionary<string, object?> filters)
        {
            if (filters == null || filters.Count == 0)
                return await GetAllAsync();

            var sqlClauses = new List<string>();
            var dbParams   = new List<DbParameter>();

            if (filters.TryGetValue("cart_id", out object? cartIdVal) && cartIdVal != null)
            {
                sqlClauses.Add("\"cart_id\" = @cart_id");
                dbParams.Add(CreateParameter("cart_id", cartIdVal));
            }

            if (filters.TryGetValue("user_id", out object? userIdVal) && userIdVal != null)
            {
                sqlClauses.Add("\"user_id\" = @user_id");
                dbParams.Add(CreateParameter("user_id", userIdVal));
            }

            if (filters.TryGetValue("medicine_id", out object? medIdVal) && medIdVal != null)
            {
                sqlClauses.Add("\"medicine_id\" = @medicine_id");
                dbParams.Add(CreateParameter("medicine_id", medIdVal));
            }

            if (sqlClauses.Count == 0)
                return await GetAllAsync();

            string sql =
                $"SELECT * FROM \"cart\" WHERE {string.Join(" AND ", sqlClauses)} " +
                "ORDER BY \"added_at\" DESC";

            return await ExecuteReaderToListAsync(sql, MapReaderToCart, dbParams.ToArray());
        }

        /// <summary>Returns a paginated slice ordered by added_at DESC.</summary>
        public async Task<PaginationModel<Cart>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize < 0)    pageSize   = 0;

            if (pageSize == 0)
            {
                var all   = await ExecuteReaderToListAsync(
                    "SELECT * FROM \"cart\" ORDER BY \"added_at\" DESC",
                    MapReaderToCart);
                int total = all.Count;
                return new PaginationModel<Cart>
                {
                    Items       = all,
                    TotalCount  = total,
                    PageSize    = total,
                    CurrentPage = 1,
                };
            }

            const string dataSql =
                "SELECT * FROM \"cart\" ORDER BY \"added_at\" DESC " +
                "LIMIT @pageSize OFFSET @offset";

            var items = await ExecuteReaderToListAsync(
                dataSql,
                MapReaderToCart,
                new[]
                {
                    CreateParameter("offset",   (pageNumber - 1) * pageSize),
                    CreateParameter("pageSize", pageSize),
                });

            var totalCount = await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM \"cart\"");

            return new PaginationModel<Cart>
            {
                Items       = items,
                TotalCount  = totalCount,
                PageSize    = pageSize,
                CurrentPage = pageNumber,
            };
        }
    }
}

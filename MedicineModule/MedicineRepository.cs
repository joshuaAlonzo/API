using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.MedicineModule
{
    public class MedicineRepository : BaseRepository, IMedicineRepository
    {
        public MedicineRepository(MyCon dbConnection) : base(dbConnection) { }

        // ---------------------------------------------------------------
        //  Private mapper
        // ---------------------------------------------------------------

        private Medicine MapReaderToMedicine(DbDataReader reader)
        {
            try
            {
                return new Medicine
                {
                    MedicineId   = ReadValue<int>(reader,      "medicine_id",   0),
                    BrandName    = ReadValue<string>(reader,   "brand_name",    string.Empty),
                    GenericName  = ReadValue<string>(reader,   "generic_name",  string.Empty),
                    CategoryId   = ReadValue<int>(reader,      "category_id",   0),
                    Description  = ReadValue<string?>(reader,  "description",   null),
                    Dosage       = ReadValue<string?>(reader,  "dosage",        null),
                    Manufacturer = ReadValue<string?>(reader,  "manufacturer",  null),
                    Price        = ReadValue<double>(reader,   "price",         0.0),
                    StockQty     = ReadValue<int>(reader,      "stock_qty",     0),
                    Image        = ReadValue<string?>(reader,  "image",         null),
                    Status       = ReadValue<string>(reader,   "status",        "Available"),
                    CreatedAt    = ReadValue<DateTime>(reader, "created_at",    DateTime.UtcNow),
                    UpdatedAt    = ReadValue<DateTime>(reader, "updated_at",    DateTime.UtcNow),
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Failed to map database row to Medicine. Check schema/type alignment.", ex);
            }
        }

        // ---------------------------------------------------------------
        //  Read operations
        // ---------------------------------------------------------------

        /// <summary>Returns all medicines ordered by brand_name.</summary>
        public async Task<IEnumerable<Medicine>> GetAllAsync()
        {
            return await ExecuteReaderToListAsync(
                "SELECT * FROM \"medicines\" ORDER BY \"brand_name\" ASC",
                MapReaderToMedicine);
        }

        /// <summary>Returns a single medicine by primary key.</summary>
        public async Task<Medicine?> GetByIdAsync(int medicineId)
        {
            var results = await ExecuteReaderToListAsync(
                "SELECT * FROM \"medicines\" WHERE \"medicine_id\" = @medicine_id",
                MapReaderToMedicine,
                new[] { CreateParameter("medicine_id", medicineId) });
            return results.FirstOrDefault();
        }

        /// <summary>Returns medicines for a specific category.</summary>
        public async Task<IEnumerable<Medicine>> GetByCategoryIdAsync(int categoryId)
        {
            return await ExecuteReaderToListAsync(
                "SELECT * FROM \"medicines\" WHERE \"category_id\" = @category_id ORDER BY \"brand_name\" ASC",
                MapReaderToMedicine,
                new[] { CreateParameter("category_id", categoryId) });
        }

        // ---------------------------------------------------------------
        //  Write operations
        // ---------------------------------------------------------------

        /// <summary>Inserts a new medicine row.</summary>
        public async Task AddAsync(Medicine entity)
        {
            const string sql =
                "INSERT INTO \"medicines\" (" +
                "\"brand_name\", \"generic_name\", \"category_id\", \"description\", \"dosage\", " +
                "\"manufacturer\", \"price\", \"stock_qty\", \"image\", \"status\"" +
                ") VALUES (" +
                "@brand_name, @generic_name, @category_id, @description, @dosage, " +
                "@manufacturer, @price, @stock_qty, @image, @status" +
                ")";

            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("brand_name",   entity.BrandName),
                CreateParameter("generic_name", entity.GenericName),
                CreateParameter("category_id",  entity.CategoryId),
                CreateParameter("description",  (object?)entity.Description  ?? DBNull.Value),
                CreateParameter("dosage",       (object?)entity.Dosage       ?? DBNull.Value),
                CreateParameter("manufacturer", (object?)entity.Manufacturer ?? DBNull.Value),
                CreateParameter("price",        entity.Price),
                CreateParameter("stock_qty",    entity.StockQty),
                CreateParameter("image",        (object?)entity.Image        ?? DBNull.Value),
                CreateParameter("status",       string.IsNullOrWhiteSpace(entity.Status) ? "Available" : entity.Status),
            });
        }

        /// <summary>Updates an existing medicine row.</summary>
        public async Task UpdateAsync(Medicine entity)
        {
            const string sql =
                "UPDATE \"medicines\" SET " +
                "\"brand_name\" = @brand_name, " +
                "\"generic_name\" = @generic_name, " +
                "\"category_id\" = @category_id, " +
                "\"description\" = @description, " +
                "\"dosage\" = @dosage, " +
                "\"manufacturer\" = @manufacturer, " +
                "\"price\" = @price, " +
                "\"stock_qty\" = @stock_qty, " +
                "\"image\" = @image, " +
                "\"status\" = @status, " +
                "\"updated_at\" = CURRENT_TIMESTAMP " +
                "WHERE \"medicine_id\" = @medicine_id";

            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("medicine_id",  entity.MedicineId),
                CreateParameter("brand_name",   entity.BrandName),
                CreateParameter("generic_name", entity.GenericName),
                CreateParameter("category_id",  entity.CategoryId),
                CreateParameter("description",  (object?)entity.Description  ?? DBNull.Value),
                CreateParameter("dosage",       (object?)entity.Dosage       ?? DBNull.Value),
                CreateParameter("manufacturer", (object?)entity.Manufacturer ?? DBNull.Value),
                CreateParameter("price",        entity.Price),
                CreateParameter("stock_qty",    entity.StockQty),
                CreateParameter("image",        (object?)entity.Image        ?? DBNull.Value),
                CreateParameter("status",       string.IsNullOrWhiteSpace(entity.Status) ? "Available" : entity.Status),
            });
        }

        // ---------------------------------------------------------------
        //  Delete operations
        // ---------------------------------------------------------------

        /// <summary>Deletes a medicine by primary key.</summary>
        public async Task DeleteAsync(int medicineId)
        {
            await ExecuteNonQueryAsync(
                "DELETE FROM \"medicines\" WHERE \"medicine_id\" = @medicine_id",
                new[] { CreateParameter("medicine_id", medicineId) });
        }

        /// <summary>Deletes all medicines.</summary>
        public async Task DeleteAllAsync()
        {
            await ExecuteNonQueryAsync("DELETE FROM \"medicines\"");
        }

        // ---------------------------------------------------------------
        //  Search / pagination
        // ---------------------------------------------------------------

        /// <summary>Partial-match search on brand_name, generic_name, manufacturer, or description.</summary>
        public async Task<IEnumerable<Medicine>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return await GetAllAsync();

            const string sql =
                "SELECT * FROM \"medicines\" WHERE " +
                "\"brand_name\" LIKE @query OR " +
                "\"generic_name\" LIKE @query OR " +
                "\"manufacturer\" LIKE @query OR " +
                "\"description\" LIKE @query " +
                "ORDER BY \"brand_name\" ASC";

            return await ExecuteReaderToListAsync(
                sql,
                MapReaderToMedicine,
                new[] { CreateParameter("query", "%" + query + "%") });
        }

        /// <summary>Returns a paginated slice with optional category and status filtering.</summary>
        public async Task<PaginationModel<Medicine>> GetPaginatedAsync(int pageNumber, int pageSize, int? categoryId = null, string? status = null)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize < 0)    pageSize   = 0;

            var whereClauses = new List<string>();
            var parameters = new List<DbParameter>();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                whereClauses.Add("\"category_id\" = @categoryId");
                parameters.Add(CreateParameter("categoryId", categoryId.Value));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                whereClauses.Add("\"status\" = @status");
                parameters.Add(CreateParameter("status", status));
            }

            string whereSql = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";

            if (pageSize == 0)
            {
                string fullSql = $"SELECT * FROM \"medicines\" {whereSql} ORDER BY \"brand_name\" ASC";
                var all = await ExecuteReaderToListAsync(fullSql, MapReaderToMedicine, parameters);
                int total = all.Count;
                return new PaginationModel<Medicine>
                {
                    Items       = all,
                    TotalCount  = total,
                    PageSize    = total,
                    CurrentPage = 1,
                };
            }

            string dataSql =
                $"SELECT * FROM \"medicines\" {whereSql} ORDER BY \"brand_name\" ASC " +
                "LIMIT @pageSize OFFSET @offset";

            var dataParameters = new List<DbParameter>(parameters)
            {
                CreateParameter("offset", (pageNumber - 1) * pageSize),
                CreateParameter("pageSize", pageSize)
            };

            var items = await ExecuteReaderToListAsync(dataSql, MapReaderToMedicine, dataParameters);

            string countSql = $"SELECT COUNT(*) FROM \"medicines\" {whereSql}";
            var totalCount = await ExecuteScalarAsync<int>(countSql, parameters);

            return new PaginationModel<Medicine>
            {
                Items       = items,
                TotalCount  = totalCount,
                PageSize    = pageSize,
                CurrentPage = pageNumber,
            };
        }
    }
}

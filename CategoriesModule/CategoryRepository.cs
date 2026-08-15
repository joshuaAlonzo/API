using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.CategoriesModule
{
    public class CategoryRepository : BaseRepository, ICategoryRepository
    {
        public CategoryRepository(MyCon dbConnection) : base(dbConnection) { }

        // ---------------------------------------------------------------
        //  Private mapper
        // ---------------------------------------------------------------

        private Category MapReaderToCategory(DbDataReader reader)
        {
            try
            {
                return new Category
                {
                    CategoryId   = ReadValue<int>(reader,    "category_id",   0),
                    CategoryName = ReadValue<string>(reader, "category_name", string.Empty),
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Failed to map database row to Category. Check schema/type alignment.", ex);
            }
        }

        // ---------------------------------------------------------------
        //  Read operations
        // ---------------------------------------------------------------

        /// <summary>Returns all categories ordered by name.</summary>
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await ExecuteReaderToListAsync(
                "SELECT * FROM \"categories\" ORDER BY \"category_name\" ASC",
                MapReaderToCategory);
        }

        /// <summary>Returns a single category by primary key.</summary>
        public async Task<Category?> GetByIdAsync(int categoryId)
        {
            var results = await ExecuteReaderToListAsync(
                "SELECT * FROM \"categories\" WHERE \"category_id\" = @category_id",
                MapReaderToCategory,
                new[] { CreateParameter("category_id", categoryId) });
            return results.FirstOrDefault();
        }

        /// <summary>Returns a category whose name matches exactly (case-insensitive).</summary>
        public async Task<Category?> GetByNameAsync(string categoryName)
        {
            var results = await ExecuteReaderToListAsync(
                "SELECT * FROM \"categories\" WHERE LOWER(\"category_name\") = LOWER(@category_name)",
                MapReaderToCategory,
                new[] { CreateParameter("category_name", categoryName) });
            return results.FirstOrDefault();
        }

        // ---------------------------------------------------------------
        //  Write operations
        // ---------------------------------------------------------------

        /// <summary>Inserts a new category row.</summary>
        public async Task AddAsync(Category entity)
        {
            const string sql =
                "INSERT INTO \"categories\" (\"category_name\") VALUES (@category_name)";

            await ExecuteNonQueryAsync(sql,
                new[] { CreateParameter("category_name", entity.CategoryName) });
        }

        /// <summary>Updates the category name for an existing row.</summary>
        public async Task UpdateAsync(Category entity)
        {
            const string sql =
                "UPDATE \"categories\" SET \"category_name\" = @category_name " +
                "WHERE \"category_id\" = @category_id";

            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("category_id",   entity.CategoryId),
                CreateParameter("category_name", entity.CategoryName),
            });
        }

        // ---------------------------------------------------------------
        //  Delete operations
        // ---------------------------------------------------------------

        /// <summary>Deletes a category by primary key.</summary>
        public async Task DeleteAsync(int categoryId)
        {
            await ExecuteNonQueryAsync(
                "DELETE FROM \"categories\" WHERE \"category_id\" = @category_id",
                new[] { CreateParameter("category_id", categoryId) });
        }

        /// <summary>Deletes every row in the categories table.</summary>
        public async Task DeleteAllAsync()
        {
            await ExecuteNonQueryAsync("DELETE FROM \"categories\"");
        }

        // ---------------------------------------------------------------
        //  Search / pagination
        // ---------------------------------------------------------------

        /// <summary>Partial-match search on category_name.</summary>
        public async Task<IEnumerable<Category>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return await GetAllAsync();

            return await ExecuteReaderToListAsync(
                "SELECT * FROM \"categories\" WHERE \"category_name\" LIKE @query ORDER BY \"category_name\" ASC",
                MapReaderToCategory,
                new[] { CreateParameter("query", "%" + query + "%") });
        }

        /// <summary>Returns a paginated slice ordered by category_name ASC.</summary>
        public async Task<PaginationModel<Category>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize < 0)    pageSize   = 0;

            if (pageSize == 0)
            {
                var all   = await ExecuteReaderToListAsync(
                    "SELECT * FROM \"categories\" ORDER BY \"category_name\" ASC",
                    MapReaderToCategory);
                int total = all.Count;
                return new PaginationModel<Category>
                {
                    Items       = all,
                    TotalCount  = total,
                    PageSize    = total,
                    CurrentPage = 1,
                };
            }

            const string dataSql =
                "SELECT * FROM \"categories\" ORDER BY \"category_name\" ASC " +
                "LIMIT @pageSize OFFSET @offset";

            var items = await ExecuteReaderToListAsync(
                dataSql,
                MapReaderToCategory,
                new[]
                {
                    CreateParameter("offset",   (pageNumber - 1) * pageSize),
                    CreateParameter("pageSize", pageSize),
                });

            var totalCount = await ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM \"categories\"");

            return new PaginationModel<Category>
            {
                Items       = items,
                TotalCount  = totalCount,
                PageSize    = pageSize,
                CurrentPage = pageNumber,
            };
        }
    }
}

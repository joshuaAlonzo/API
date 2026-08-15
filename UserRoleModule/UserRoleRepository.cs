using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Common;
using Api.Main;

namespace Api.UserRoleModule
{
    public class UserroleRepository : BaseRepository, IUserroleRepository
    {
        public UserroleRepository(MyCon dbConnection) : base(dbConnection) { }

        private Userrole MapReaderToUserrole(DbDataReader reader)
        {
            try
            {
                return new Userrole()
                {
                    UserRoleId = ReadValue<int>(reader, "role_id", 0),
                    UserRole = ReadValue<string>(reader, "role_name", string.Empty),
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Failed to map database row to Userrole. Check schema/type alignment for generated columns.",
                    ex);
            }
        }

        /// <summary>
        /// Retrieves all entities from the database table "roles"
        /// </summary>
        public async Task<IEnumerable<Userrole>> GetAllAsync()
        {
            return await ExecuteReaderToListAsync("SELECT * FROM \"roles\"", MapReaderToUserrole);
        }

        /// <summary>
        /// Retrieves an entity by role_id
        /// </summary>
        public async Task<Userrole?> GetByIdAsync(int userRoleId)
        {
            var results = await ExecuteReaderToListAsync(
                "SELECT * FROM \"roles\" WHERE \"role_id\" = @role_id", 
                MapReaderToUserrole, 
                new[] { CreateParameter("role_id", userRoleId) });
            return results.FirstOrDefault();
        }

        /// <summary>
        /// Adds a new entity to the database table "roles"
        /// </summary>
        public async Task AddAsync(Userrole entity)
        {
            var sql = "INSERT INTO \"roles\" (\"role_name\") VALUES (@role_name)";
            var dbParameters = new List<DbParameter>();
            dbParameters.Add(CreateParameter("role_name", entity.UserRole));
            await ExecuteNonQueryAsync(sql, dbParameters.ToArray());
        }

        /// <summary>
        /// Updates an existing entity in the database table "roles"
        /// </summary>
        public async Task UpdateAsync(Userrole entity)
        {
            var sql = "UPDATE \"roles\" SET \"role_name\" = @role_name WHERE \"role_id\" = @role_id";
            var dbParameters = new List<DbParameter>();
            dbParameters.Add(CreateParameter("role_id", entity.UserRoleId));
            dbParameters.Add(CreateParameter("role_name", entity.UserRole));
            await ExecuteNonQueryAsync(sql, dbParameters.ToArray());
        }

        /// <summary>
        /// Deletes an entity from the database by its role_id
        /// </summary>
        public async Task DeleteAsync(int userRoleId)
        {
            await ExecuteNonQueryAsync("DELETE FROM \"roles\" WHERE \"role_id\" = @role_id", new[] { CreateParameter("role_id", userRoleId) });
        }

        /// <summary>
        /// Deletes all entities from the "roles" table
        /// </summary>
        public async Task DeleteAllAsync()
        {
            await ExecuteNonQueryAsync("DELETE FROM \"roles\"");
        }

        private static int? GetFilterInt(IReadOnlyDictionary<string, object?> filters, string key)
        {
            if (!filters.TryGetValue(key, out object? rawValue) || rawValue == null)
            {
                return null;
            }

            if (rawValue is int intValue)
            {
                return intValue;
            }

            string text = Convert.ToString(rawValue, CultureInfo.InvariantCulture) ?? string.Empty;
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed)
                ? parsed
                : null;
        }

        private static string? GetFilterText(IReadOnlyDictionary<string, object?> filters, string key)
        {
            if (!filters.TryGetValue(key, out object? rawValue) || rawValue == null)
            {
                return null;
            }

            string text = (Convert.ToString(rawValue, CultureInfo.InvariantCulture) ?? string.Empty).Trim();
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }

        public async Task<IEnumerable<Userrole>> GetFilteredExactAsync(IReadOnlyDictionary<string, object?> filters)
        {
            if (filters == null || filters.Count == 0)
            {
                return await GetAllAsync();
            }

            var sqlFilters = new List<string>();
            var dbParameters = new List<DbParameter>();

            if ((filters.TryGetValue("userRoleId", out object? roleIdFilter) && roleIdFilter != null) ||
                (filters.TryGetValue("roleId", out roleIdFilter) && roleIdFilter != null) ||
                (filters.TryGetValue("role_id", out roleIdFilter) && roleIdFilter != null))
            {
                sqlFilters.Add("\"role_id\" = @role_id");
                dbParameters.Add(CreateParameter("role_id", roleIdFilter));
            }

            if ((filters.TryGetValue("userRole", out object? roleNameFilter) && roleNameFilter != null) ||
                (filters.TryGetValue("roleName", out roleNameFilter) && roleNameFilter != null) ||
                (filters.TryGetValue("role_name", out roleNameFilter) && roleNameFilter != null))
            {
                sqlFilters.Add("\"role_name\" = @role_name");
                dbParameters.Add(CreateParameter("role_name", roleNameFilter));
            }

            IEnumerable<Userrole> queryable = sqlFilters.Count == 0
                ? await GetAllAsync()
                : await ExecuteReaderToListAsync(
                    $"SELECT * FROM \"roles\" WHERE {string.Join(" AND ", sqlFilters)}",
                    MapReaderToUserrole,
                    dbParameters.ToArray());
            return queryable.ToList();
        }

        public async Task<IEnumerable<Userrole>> GetFilteredLikeAsync(IReadOnlyDictionary<string, object?> filters)
        {
            if (filters == null || filters.Count == 0)
            {
                return await GetAllAsync();
            }

            var sqlFilters = new List<string>();
            var dbParameters = new List<DbParameter>();

            string? roleIdFilter = GetFilterText(filters, "userRoleId") 
                ?? GetFilterText(filters, "roleId") 
                ?? GetFilterText(filters, "role_id");

            if (!string.IsNullOrWhiteSpace(roleIdFilter))
            {
                string roleIdPattern = "%" + roleIdFilter + "%";
                sqlFilters.Add("CAST(\"role_id\" AS TEXT) LIKE @roleIdPattern");
                dbParameters.Add(CreateParameter("roleIdPattern", roleIdPattern));
            }

            string? roleNameFilter = GetFilterText(filters, "userRole") 
                ?? GetFilterText(filters, "roleName") 
                ?? GetFilterText(filters, "role_name");

            if (!string.IsNullOrWhiteSpace(roleNameFilter))
            {
                string roleNamePattern = "%" + roleNameFilter + "%";
                sqlFilters.Add("\"role_name\" LIKE @roleNamePattern");
                dbParameters.Add(CreateParameter("roleNamePattern", roleNamePattern));
            }

            IEnumerable<Userrole> queryable = sqlFilters.Count == 0
                ? await GetAllAsync()
                : await ExecuteReaderToListAsync(
                    $"SELECT * FROM \"roles\" WHERE {string.Join(" AND ", sqlFilters)}",
                    MapReaderToUserrole,
                    dbParameters.ToArray());
            return queryable.ToList();
        }

        /// <summary>
        /// Searches for entities by query string
        /// </summary>
        public async Task<IEnumerable<Userrole>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await GetAllAsync();
            }
            return await SearchAsyncAll(query);
        }

        /// <summary>
        /// Searches for entities by all columns
        /// </summary>
        public async Task<IEnumerable<Userrole>> SearchAsyncAll(string query)
        {
            var filters = new List<string>();
            filters.Add("CAST(\"role_id\" AS TEXT) LIKE @query");
            filters.Add("\"role_name\" LIKE @query");

            string sql = $"SELECT * FROM \"roles\" WHERE {string.Join(" OR ", filters)}";
            return await ExecuteReaderToListAsync(sql, MapReaderToUserrole, new[] { CreateParameter("query", "%" + query + "%") });
        }

        /// <summary>
        /// Retrieves a paginated list of entities from table "roles"
        /// </summary>
        public async Task<PaginationModel<Userrole>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
            {
                pageNumber = 1;
            }

            if (pageSize < 0)
            {
                pageSize = 0;
            }

            if (pageSize <= 0)
            {
                var allItems = await ExecuteReaderToListAsync("SELECT * FROM \"roles\"", MapReaderToUserrole);
                var totalRecords = allItems.Count;
                return new PaginationModel<Userrole>()
                {
                    Items = allItems,
                    TotalCount = totalRecords,
                    PageSize = totalRecords,
                    CurrentPage = 1
                };
            }

            string sql = "SELECT * FROM \"roles\" ORDER BY \"role_id\" LIMIT @pageSize OFFSET @offset";
            var items = await ExecuteReaderToListAsync(sql, MapReaderToUserrole, new[]
            {
                CreateParameter("offset", (pageNumber - 1) * pageSize),
                CreateParameter("pageSize", pageSize)
            });

            var totalRecordsCount = await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM \"roles\"");
            return new PaginationModel<Userrole>()
            {
                Items = items,
                TotalCount = totalRecordsCount,
                PageSize = pageSize,
                CurrentPage = pageNumber
            };
        }

        /// <summary>
        /// Uploads data from a list to the database table "roles".
        /// </summary>
        public async Task BulkUploadAsync(List<Userrole> dataList)
        {
            if (dataList == null || dataList.Count == 0) return;

            foreach (var item in dataList)
            {
                await AddAsync(item);
            }
        }
    }
}

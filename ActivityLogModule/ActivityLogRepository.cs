using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.ActivityLogModule
{
    public class ActivityLogRepository : BaseRepository, IActivityLogRepository
    {
        public ActivityLogRepository(MyCon dbConnection) : base(dbConnection) { }

        // ---------------------------------------------------------------
        //  Private mapper
        // ---------------------------------------------------------------

        private ActivityLog MapReaderToActivityLog(DbDataReader reader)
        {
            try
            {
                return new ActivityLog
                {
                    LogId        = ReadValue<int>(reader,      "log_id",       0),
                    UserId       = ReadValue<int>(reader,      "user_id",      0),
                    Activity     = ReadValue<string>(reader,   "activity",     string.Empty),
                    ActivityDate = ReadValue<DateTime>(reader, "activity_date", DateTime.MinValue),
                    IpAddress    = reader.IsDBNull(reader.GetOrdinal("ip_address"))
                                       ? null
                                       : reader.GetString(reader.GetOrdinal("ip_address")),
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Failed to map database row to ActivityLog. Check schema/type alignment.", ex);
            }
        }

        // ---------------------------------------------------------------
        //  CRUD
        // ---------------------------------------------------------------

        /// <summary>Returns all rows from activity_logs.</summary>
        public async Task<IEnumerable<ActivityLog>> GetAllAsync()
        {
            return await ExecuteReaderToListAsync(
                "SELECT * FROM \"activity_logs\" ORDER BY \"activity_date\" DESC",
                MapReaderToActivityLog);
        }

        /// <summary>Returns a single log by primary key.</summary>
        public async Task<ActivityLog?> GetByIdAsync(int logId)
        {
            var results = await ExecuteReaderToListAsync(
                "SELECT * FROM \"activity_logs\" WHERE \"log_id\" = @log_id",
                MapReaderToActivityLog,
                new[] { CreateParameter("log_id", logId) });
            return results.FirstOrDefault();
        }

        /// <summary>Returns all logs that belong to a specific user.</summary>
        public async Task<IEnumerable<ActivityLog>> GetByUserIdAsync(int userId)
        {
            return await ExecuteReaderToListAsync(
                "SELECT * FROM \"activity_logs\" WHERE \"user_id\" = @user_id ORDER BY \"activity_date\" DESC",
                MapReaderToActivityLog,
                new[] { CreateParameter("user_id", userId) });
        }

        /// <summary>Inserts a new activity log row.</summary>
        public async Task AddAsync(ActivityLog entity)
        {
            const string sql =
                "INSERT INTO \"activity_logs\" (\"user_id\", \"activity\", \"activity_date\", \"ip_address\") " +
                "VALUES (@user_id, @activity, @activity_date, @ip_address)";

            var parameters = new[]
            {
                CreateParameter("user_id",       entity.UserId),
                CreateParameter("activity",      entity.Activity),
                CreateParameter("activity_date", entity.ActivityDate == DateTime.MinValue
                                                    ? (object)DBNull.Value
                                                    : entity.ActivityDate),
                CreateParameter("ip_address",    (object?)entity.IpAddress ?? DBNull.Value),
            };

            await ExecuteNonQueryAsync(sql, parameters);
        }

        /// <summary>Deletes one log by primary key.</summary>
        public async Task DeleteAsync(int logId)
        {
            await ExecuteNonQueryAsync(
                "DELETE FROM \"activity_logs\" WHERE \"log_id\" = @log_id",
                new[] { CreateParameter("log_id", logId) });
        }

        /// <summary>Deletes all logs belonging to a specific user.</summary>
        public async Task DeleteByUserIdAsync(int userId)
        {
            await ExecuteNonQueryAsync(
                "DELETE FROM \"activity_logs\" WHERE \"user_id\" = @user_id",
                new[] { CreateParameter("user_id", userId) });
        }

        /// <summary>Deletes every row in activity_logs.</summary>
        public async Task DeleteAllAsync()
        {
            await ExecuteNonQueryAsync("DELETE FROM \"activity_logs\"");
        }

        // ---------------------------------------------------------------
        //  Filtered queries
        // ---------------------------------------------------------------

        /// <summary>
        /// Exact-match filter. Supported keys: log_id, user_id, activity, ip_address.
        /// </summary>
        public async Task<IEnumerable<ActivityLog>> GetFilteredExactAsync(
            IReadOnlyDictionary<string, object?> filters)
        {
            if (filters == null || filters.Count == 0)
                return await GetAllAsync();

            var sqlClauses = new List<string>();
            var dbParams   = new List<DbParameter>();

            if (filters.TryGetValue("log_id", out object? logIdVal) && logIdVal != null)
            {
                sqlClauses.Add("\"log_id\" = @log_id");
                dbParams.Add(CreateParameter("log_id", logIdVal));
            }

            if (filters.TryGetValue("user_id", out object? userIdVal) && userIdVal != null)
            {
                sqlClauses.Add("\"user_id\" = @user_id");
                dbParams.Add(CreateParameter("user_id", userIdVal));
            }

            if (filters.TryGetValue("activity", out object? activityVal) && activityVal != null)
            {
                string activity = Convert.ToString(activityVal, CultureInfo.InvariantCulture) ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(activity))
                {
                    sqlClauses.Add("LOWER(\"activity\") = LOWER(@activity)");
                    dbParams.Add(CreateParameter("activity", activity));
                }
            }

            if (filters.TryGetValue("ip_address", out object? ipVal) && ipVal != null)
            {
                string ip = Convert.ToString(ipVal, CultureInfo.InvariantCulture) ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(ip))
                {
                    sqlClauses.Add("\"ip_address\" = @ip_address");
                    dbParams.Add(CreateParameter("ip_address", ip));
                }
            }

            if (sqlClauses.Count == 0)
                return await GetAllAsync();

            string sql = $"SELECT * FROM \"activity_logs\" WHERE {string.Join(" AND ", sqlClauses)} " +
                         "ORDER BY \"activity_date\" DESC";

            return await ExecuteReaderToListAsync(sql, MapReaderToActivityLog, dbParams.ToArray());
        }

        /// <summary>
        /// LIKE-based (partial-match) filter. Supported keys: activity, ip_address.
        /// </summary>
        public async Task<IEnumerable<ActivityLog>> GetFilteredLikeAsync(
            IReadOnlyDictionary<string, object?> filters)
        {
            if (filters == null || filters.Count == 0)
                return await GetAllAsync();

            var sqlClauses = new List<string>();
            var dbParams   = new List<DbParameter>();

            if (filters.TryGetValue("activity", out object? activityVal) && activityVal != null)
            {
                string activity = (Convert.ToString(activityVal, CultureInfo.InvariantCulture) ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(activity))
                {
                    sqlClauses.Add("\"activity\" LIKE @activityPattern");
                    dbParams.Add(CreateParameter("activityPattern", "%" + activity + "%"));
                }
            }

            if (filters.TryGetValue("ip_address", out object? ipVal) && ipVal != null)
            {
                string ip = (Convert.ToString(ipVal, CultureInfo.InvariantCulture) ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(ip))
                {
                    sqlClauses.Add("\"ip_address\" LIKE @ipPattern");
                    dbParams.Add(CreateParameter("ipPattern", "%" + ip + "%"));
                }
            }

            if (sqlClauses.Count == 0)
                return await GetAllAsync();

            string sql = $"SELECT * FROM \"activity_logs\" WHERE {string.Join(" AND ", sqlClauses)} " +
                         "ORDER BY \"activity_date\" DESC";

            return await ExecuteReaderToListAsync(sql, MapReaderToActivityLog, dbParams.ToArray());
        }

        /// <summary>
        /// Full-text search across activity and ip_address columns.
        /// </summary>
        public async Task<IEnumerable<ActivityLog>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return await GetAllAsync();

            const string sql =
                "SELECT * FROM \"activity_logs\" " +
                "WHERE \"activity\" LIKE @query OR \"ip_address\" LIKE @query " +
                "ORDER BY \"activity_date\" DESC";

            return await ExecuteReaderToListAsync(
                sql,
                MapReaderToActivityLog,
                new[] { CreateParameter("query", "%" + query + "%") });
        }

        // ---------------------------------------------------------------
        //  Pagination
        // ---------------------------------------------------------------

        /// <summary>Returns a paginated slice ordered by activity_date DESC.</summary>
        public async Task<PaginationModel<ActivityLog>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize < 0)    pageSize   = 0;

            if (pageSize == 0)
            {
                var all    = await ExecuteReaderToListAsync(
                    "SELECT * FROM \"activity_logs\" ORDER BY \"activity_date\" DESC",
                    MapReaderToActivityLog);
                int total  = all.Count;
                return new PaginationModel<ActivityLog>
                {
                    Items       = all,
                    TotalCount  = total,
                    PageSize    = total,
                    CurrentPage = 1,
                };
            }

            const string dataSql =
                "SELECT * FROM \"activity_logs\" ORDER BY \"activity_date\" DESC " +
                "LIMIT @pageSize OFFSET @offset";

            var items = await ExecuteReaderToListAsync(
                dataSql,
                MapReaderToActivityLog,
                new[]
                {
                    CreateParameter("offset",   (pageNumber - 1) * pageSize),
                    CreateParameter("pageSize", pageSize),
                });

            var totalCount = await ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM \"activity_logs\"");

            return new PaginationModel<ActivityLog>
            {
                Items       = items,
                TotalCount  = totalCount,
                PageSize    = pageSize,
                CurrentPage = pageNumber,
            };
        }
    }
}

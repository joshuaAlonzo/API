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
                int logId = ReadValue<int>(reader, "log_id", 0);
                if (logId == 0) logId = ReadValue<int>(reader, "id", 0);
                if (logId == 0) logId = ReadValue<int>(reader, "activity_id", 0);
                if (logId == 0) logId = ReadValue<int>(reader, "activity_log_id", 0);

                int userId = ReadValue<int>(reader, "user_id", 0);
                if (userId == 0) userId = ReadValue<int>(reader, "userId", 0);
                if (userId == 0) userId = ReadValue<int>(reader, "admin_id", 0);

                string activity = ReadValue<string>(reader, "activity", string.Empty);
                if (string.IsNullOrEmpty(activity)) activity = ReadValue<string>(reader, "action", string.Empty);
                if (string.IsNullOrEmpty(activity)) activity = ReadValue<string>(reader, "description", string.Empty);
                if (string.IsNullOrEmpty(activity)) activity = ReadValue<string>(reader, "details", string.Empty);
                if (string.IsNullOrEmpty(activity)) activity = ReadValue<string>(reader, "message", string.Empty);

                DateTime activityDate = ReadValue<DateTime>(reader, "activity_date", DateTime.MinValue);
                if (activityDate == DateTime.MinValue) activityDate = ReadValue<DateTime>(reader, "created_at", DateTime.MinValue);
                if (activityDate == DateTime.MinValue) activityDate = ReadValue<DateTime>(reader, "timestamp", DateTime.MinValue);
                if (activityDate == DateTime.MinValue) activityDate = ReadValue<DateTime>(reader, "date", DateTime.MinValue);
                if (activityDate == DateTime.MinValue) activityDate = ReadValue<DateTime>(reader, "log_date", DateTime.MinValue);
                if (activityDate == DateTime.MinValue) activityDate = DateTime.UtcNow;

                string? ipAddress = ReadValue<string?>(reader, "ip_address", null);
                if (string.IsNullOrEmpty(ipAddress)) ipAddress = ReadValue<string?>(reader, "ip", null);
                if (string.IsNullOrEmpty(ipAddress)) ipAddress = ReadValue<string?>(reader, "ipaddress", null);

                return new ActivityLog
                {
                    LogId        = logId,
                    UserId       = userId,
                    Activity     = activity,
                    ActivityDate = activityDate,
                    IpAddress    = string.IsNullOrWhiteSpace(ipAddress) ? null : ipAddress,
                };
            }
            catch (Exception ex)
            {
                var colDetails = new List<string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    colDetails.Add($"{reader.GetName(i)}={reader.GetValue(i)} ({reader.GetFieldType(i).Name})");
                }
                throw new InvalidOperationException(
                    $"Failed to map database row to ActivityLog. Columns: [{string.Join(", ", colDetails)}]. Error: {ex.Message}", ex);
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
            var dateToSave = entity.ActivityDate == DateTime.MinValue ? DateTime.UtcNow : entity.ActivityDate;

            const string sql =
                "INSERT INTO \"activity_logs\" (\"user_id\", \"activity\", \"activity_date\", \"ip_address\") " +
                "VALUES (@user_id, @activity, @activity_date, @ip_address)";

            var parameters = new[]
            {
                CreateParameter("user_id",       entity.UserId),
                CreateParameter("activity",      entity.Activity),
                CreateParameter("activity_date", dateToSave.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)),
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

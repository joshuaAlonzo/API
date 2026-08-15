using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Common;
using Api.Main;

namespace Api.UserModule
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(MyCon dbConnection) : base(dbConnection) { }

        private User MapReaderToUser(DbDataReader reader)
        {
            try
            {
                return new User
                {
                    UserId        = ReadValue<int>(reader,      "user_id",        0),
                    FirstName     = ReadValue<string>(reader,   "first_name",     string.Empty),
                    LastName      = ReadValue<string>(reader,   "last_name",      string.Empty),
                    Email         = ReadValue<string>(reader,   "email",          string.Empty),
                    ContactNumber = ReadValue<string?>(reader,  "contact_number", null),
                    Username      = ReadValue<string>(reader,   "username",       string.Empty),
                    PasswordHash  = ReadValue<string>(reader,   "password_hash",  string.Empty),
                    RoleId        = ReadValue<int>(reader,      "role_id",        0),
                    CreatedAt     = ReadValue<DateTime>(reader, "created_at",     DateTime.MinValue),
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Failed to map database row to User. Check schema/type alignment.", ex);
            }
        }

        // ---------------------------------------------------------------
        //  Read
        // ---------------------------------------------------------------

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await ExecuteReaderToListAsync(
                "SELECT * FROM \"users\" ORDER BY \"user_id\" ASC",
                MapReaderToUser);
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            var results = await ExecuteReaderToListAsync(
                "SELECT * FROM \"users\" WHERE \"user_id\" = @user_id",
                MapReaderToUser,
                new[] { CreateParameter("user_id", userId) });
            return results.FirstOrDefault();
        }

        /// <summary>Looks up a user by their username (unique column in pharmacy.db).</summary>
        public async Task<User?> GetByUserNameAsync(string userName)
        {
            var results = await ExecuteReaderToListAsync(
                "SELECT * FROM \"users\" WHERE \"username\" = @username COLLATE NOCASE",
                MapReaderToUser,
                new[] { CreateParameter("username", userName) });
            return results.FirstOrDefault();
        }

        // ---------------------------------------------------------------
        //  Write
        // ---------------------------------------------------------------

        /// <summary>Inserts a new user. users table has no updated_at column.</summary>
        public async Task AddAsync(User entity)
        {
            const string sql =
                "INSERT INTO \"users\" " +
                "(\"first_name\", \"last_name\", \"email\", \"contact_number\", \"username\", \"password_hash\", \"role_id\") " +
                "VALUES (@first_name, @last_name, @email, @contact_number, @username, @password_hash, @role_id)";

            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("first_name",     entity.FirstName),
                CreateParameter("last_name",      entity.LastName),
                CreateParameter("email",          entity.Email),
                CreateParameter("contact_number", (object?)entity.ContactNumber ?? DBNull.Value),
                CreateParameter("username",       entity.Username),
                CreateParameter("password_hash",  entity.PasswordHash),
                CreateParameter("role_id",        entity.RoleId),
            });
        }

        /// <summary>Updates an existing user. users table has no updated_at column.</summary>
        public async Task UpdateAsync(User entity)
        {
            const string sql =
                "UPDATE \"users\" SET " +
                "\"first_name\" = @first_name, " +
                "\"last_name\" = @last_name, " +
                "\"email\" = @email, " +
                "\"contact_number\" = @contact_number, " +
                "\"username\" = @username, " +
                "\"password_hash\" = @password_hash, " +
                "\"role_id\" = @role_id " +
                "WHERE \"user_id\" = @user_id";

            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("user_id",        entity.UserId),
                CreateParameter("first_name",     entity.FirstName),
                CreateParameter("last_name",      entity.LastName),
                CreateParameter("email",          entity.Email),
                CreateParameter("contact_number", (object?)entity.ContactNumber ?? DBNull.Value),
                CreateParameter("username",       entity.Username),
                CreateParameter("password_hash",  entity.PasswordHash),
                CreateParameter("role_id",        entity.RoleId),
            });
        }

        public async Task DeleteAsync(int userId)
        {
            await ExecuteNonQueryAsync(
                "DELETE FROM \"users\" WHERE \"user_id\" = @user_id",
                new[] { CreateParameter("user_id", userId) });
        }

        public async Task DeleteAllAsync()
        {
            await ExecuteNonQueryAsync("DELETE FROM \"users\"");
        }

        // ---------------------------------------------------------------
        //  Filter / Search
        // ---------------------------------------------------------------

        public async Task<IEnumerable<User>> GetFilteredExactAsync(IReadOnlyDictionary<string, object?> filters)
        {
            if (filters == null || filters.Count == 0)
                return await GetAllAsync();

            var clauses = new List<string>();
            var dbParams = new List<DbParameter>();

            if (filters.TryGetValue("user_id", out object? uId) && uId != null)
            {
                clauses.Add("\"user_id\" = @user_id");
                dbParams.Add(CreateParameter("user_id", uId));
            }
            if (filters.TryGetValue("username", out object? uname) && uname != null)
            {
                clauses.Add("LOWER(\"username\") = LOWER(@username)");
                dbParams.Add(CreateParameter("username", uname));
            }
            if (filters.TryGetValue("email", out object? email) && email != null)
            {
                clauses.Add("LOWER(\"email\") = LOWER(@email)");
                dbParams.Add(CreateParameter("email", email));
            }
            if (filters.TryGetValue("role_id", out object? rid) && rid != null)
            {
                clauses.Add("\"role_id\" = @role_id");
                dbParams.Add(CreateParameter("role_id", rid));
            }

            if (clauses.Count == 0) return await GetAllAsync();

            string sql = $"SELECT * FROM \"users\" WHERE {string.Join(" AND ", clauses)} ORDER BY \"user_id\" ASC";
            return await ExecuteReaderToListAsync(sql, MapReaderToUser, dbParams);
        }

        public async Task<IEnumerable<User>> GetFilteredLikeAsync(IReadOnlyDictionary<string, object?> filters)
        {
            if (filters == null || filters.Count == 0)
                return await GetAllAsync();

            var clauses = new List<string>();
            var dbParams = new List<DbParameter>();

            if (filters.TryGetValue("userId", out object? uId) && uId != null)
            {
                clauses.Add("CAST(\"user_id\" AS TEXT) LIKE @userIdPattern");
                dbParams.Add(CreateParameter("userIdPattern", "%" + uId + "%"));
            }
            if (filters.TryGetValue("username", out object? uname) && uname != null)
            {
                clauses.Add("\"username\" LIKE @usernamePattern");
                dbParams.Add(CreateParameter("usernamePattern", "%" + uname + "%"));
            }

            if (clauses.Count == 0) return await GetAllAsync();

            string sql = $"SELECT * FROM \"users\" WHERE {string.Join(" AND ", clauses)} ORDER BY \"user_id\" ASC";
            return await ExecuteReaderToListAsync(sql, MapReaderToUser, dbParams);
        }

        public async Task<IEnumerable<User>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return await GetAllAsync();

            return await SearchAsyncAll(query);
        }

        public async Task<IEnumerable<User>> SearchAsyncAll(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return await GetAllAsync();

            const string sql =
                "SELECT * FROM \"users\" WHERE " +
                "\"username\" LIKE @query OR " +
                "\"first_name\" LIKE @query OR " +
                "\"last_name\" LIKE @query OR " +
                "\"email\" LIKE @query " +
                "ORDER BY \"user_id\" ASC";

            return await ExecuteReaderToListAsync(
                sql, MapReaderToUser,
                new[] { CreateParameter("query", "%" + query + "%") });
        }

        // ---------------------------------------------------------------
        //  Pagination
        // ---------------------------------------------------------------

        public async Task<PaginationModel<User>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize < 0)    pageSize   = 0;

            if (pageSize == 0)
            {
                var all = await ExecuteReaderToListAsync(
                    "SELECT * FROM \"users\" ORDER BY \"user_id\" ASC", MapReaderToUser);
                return new PaginationModel<User>
                {
                    Items       = all,
                    TotalCount  = all.Count,
                    PageSize    = all.Count,
                    CurrentPage = 1,
                };
            }

            var items = await ExecuteReaderToListAsync(
                "SELECT * FROM \"users\" ORDER BY \"user_id\" ASC LIMIT @pageSize OFFSET @offset",
                MapReaderToUser,
                new[]
                {
                    CreateParameter("offset",   (pageNumber - 1) * pageSize),
                    CreateParameter("pageSize", pageSize),
                });

            var totalCount = await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM \"users\"");

            return new PaginationModel<User>
            {
                Items       = items,
                TotalCount  = totalCount,
                PageSize    = pageSize,
                CurrentPage = pageNumber,
            };
        }

        // ---------------------------------------------------------------
        //  Bulk
        // ---------------------------------------------------------------

        public async Task BulkUploadAsync(List<User> dataList)
        {
            if (dataList == null || dataList.Count == 0) return;
            foreach (var item in dataList)
                await AddAsync(item);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Main;

namespace Api.ActivityLogModule
{
    public interface IActivityLogRepository
    {
        /// <summary>Retrieves all activity logs.</summary>
        Task<IEnumerable<ActivityLog>> GetAllAsync();

        /// <summary>Retrieves an activity log by its primary key.</summary>
        Task<ActivityLog?> GetByIdAsync(int logId);

        /// <summary>Retrieves all activity logs for a specific user.</summary>
        Task<IEnumerable<ActivityLog>> GetByUserIdAsync(int userId);

        /// <summary>Adds a new activity log entry.</summary>
        Task AddAsync(ActivityLog entity);

        /// <summary>Deletes an activity log by its primary key.</summary>
        Task DeleteAsync(int logId);

        /// <summary>Deletes all activity logs for a specific user.</summary>
        Task DeleteByUserIdAsync(int userId);

        /// <summary>Deletes all activity logs.</summary>
        Task DeleteAllAsync();

        /// <summary>Exact-match filter query.</summary>
        Task<IEnumerable<ActivityLog>> GetFilteredExactAsync(IReadOnlyDictionary<string, object?> filters);

        /// <summary>LIKE-based filter query.</summary>
        Task<IEnumerable<ActivityLog>> GetFilteredLikeAsync(IReadOnlyDictionary<string, object?> filters);

        /// <summary>Full-text search across all text columns.</summary>
        Task<IEnumerable<ActivityLog>> SearchAsync(string query);

        /// <summary>Returns a paginated result set.</summary>
        Task<PaginationModel<ActivityLog>> GetPaginatedAsync(int pageNumber, int pageSize);
    }
}

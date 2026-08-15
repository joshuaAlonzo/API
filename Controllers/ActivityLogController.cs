using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.ActivityLogModule;
using Api.Main;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class ActivityLogController : ControllerBase
    {
        private readonly IActivityLogRepository _repository;

        public ActivityLogController(IActivityLogRepository repository)
        {
            _repository = repository;
        }

        // ---------------------------------------------------------------
        //  Request / Response shapes
        // ---------------------------------------------------------------

        public sealed class CreateActivityLogRequest
        {
            public int    UserId    { get; set; }
            public string Activity  { get; set; } = string.Empty;
            public string? IpAddress { get; set; }
        }

        public sealed class SearchByFilterRequest
        {
            public int?    LogId     { get; set; }
            public int?    UserId    { get; set; }
            public string? Activity  { get; set; }
            public string? IpAddress { get; set; }
        }

        private static Dictionary<string, object?> BuildFilterMap(SearchByFilterRequest request)
        {
            var filters = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            if (request.LogId    != null) filters["log_id"]    = request.LogId;
            if (request.UserId   != null) filters["user_id"]   = request.UserId;
            if (request.Activity != null) filters["activity"]  = request.Activity;
            if (request.IpAddress != null) filters["ip_address"] = request.IpAddress;
            return filters;
        }

        // ---------------------------------------------------------------
        //  GET endpoints
        // ---------------------------------------------------------------

        /// <summary>Get all activity logs (AdminAccess only)</summary>
        [HttpGet]
        [Authorize(Policy = "AdminAccess")]
        public async Task<ActionResult<IEnumerable<ActivityLog>>> GetAll()
        {
            try
            {
                var result = await _repository.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Get a single activity log by ID (AdminAccess only)</summary>
        [HttpGet("{id:int}")]
        [Authorize(Policy = "AdminAccess")]
        public async Task<ActionResult<ActivityLog>> GetById(int id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                    return NotFound(new { message = "Activity log not found." });
                return Ok(entity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Get all activity logs for a specific user (ModAccess only)</summary>
        [HttpGet("user/{userId:int}")]
        [Authorize(Policy = "ModAccess")]
        public async Task<ActionResult<IEnumerable<ActivityLog>>> GetByUserId(int userId)
        {
            try
            {
                var result = await _repository.GetByUserIdAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Exact-match filter search (AdminAccess only)</summary>
        [HttpGet("search-by-filter-ematch")]
        [Authorize(Policy = "AdminAccess")]
        public async Task<ActionResult<IEnumerable<ActivityLog>>> SearchByFilterExact(
            [FromQuery] SearchByFilterRequest request)
        {
            try
            {
                IReadOnlyDictionary<string, object?> filters = BuildFilterMap(request);
                if (filters.Count == 0)
                    return BadRequest(new { message = "At least one filter must be provided." });

                var result = await _repository.GetFilteredExactAsync(filters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Partial-match (LIKE) filter search (AdminAccess only)</summary>
        [HttpGet("search-by-filter-lmatch")]
        [Authorize(Policy = "AdminAccess")]
        public async Task<ActionResult<IEnumerable<ActivityLog>>> SearchByFilterLike(
            [FromQuery] SearchByFilterRequest request)
        {
            try
            {
                IReadOnlyDictionary<string, object?> filters = BuildFilterMap(request);
                if (filters.Count == 0)
                    return BadRequest(new { message = "At least one filter must be provided." });

                var result = await _repository.GetFilteredLikeAsync(filters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Full-text search across activity and ip_address (AdminAccess only)</summary>
        [HttpGet("search")]
        [Authorize(Policy = "AdminAccess")]
        public async Task<ActionResult<IEnumerable<ActivityLog>>> Search([FromQuery] string query)
        {
            try
            {
                var result = await _repository.SearchAsync(query ?? string.Empty);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Paginated list of activity logs (AdminAccess only)</summary>
        [HttpGet("paginated")]
        [Authorize(Policy = "AdminAccess")]
        public async Task<ActionResult> GetPaginated(int pageNumber = 1, int pageSize = 25)
        {
            try
            {
                var pagedResult = await _repository.GetPaginatedAsync(pageNumber, pageSize);
                var response = new PaginationModel<ActivityLog>
                {
                    Items       = pagedResult.Items,
                    TotalCount  = pagedResult.TotalCount,
                    PageSize    = pagedResult.PageSize,
                    CurrentPage = pagedResult.CurrentPage,
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ---------------------------------------------------------------
        //  POST endpoints
        // ---------------------------------------------------------------

        /// <summary>Create a new activity log entry (ModAccess only)</summary>
        [HttpPost]
        [Authorize(Policy = "ModAccess")]
        public async Task<IActionResult> Create([FromBody] CreateActivityLogRequest request)
        {
            try
            {
                if (request.UserId <= 0)
                    return BadRequest(new { message = "A valid UserId is required." });
                if (string.IsNullOrWhiteSpace(request.Activity))
                    return BadRequest(new { message = "Activity description is required." });

                var entity = new ActivityLog
                {
                    UserId       = request.UserId,
                    Activity     = request.Activity,
                    ActivityDate = DateTime.UtcNow,
                    IpAddress    = request.IpAddress,
                };

                await _repository.AddAsync(entity);
                return Ok(new { message = "Activity log created successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ---------------------------------------------------------------
        //  DELETE endpoints
        // ---------------------------------------------------------------

        /// <summary>Delete a single activity log by ID (AdminAccess only)</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { message = "Activity log not found." });

                await _repository.DeleteAsync(id);
                return Ok(new { message = "Activity log deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Delete all logs for a specific user (AdminAccess only)</summary>
        [HttpDelete("user/{userId:int}")]
        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> DeleteByUserId(int userId)
        {
            try
            {
                await _repository.DeleteByUserIdAsync(userId);
                return Ok(new { message = $"All activity logs for user {userId} deleted." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Delete all activity logs (AdminAccess only)</summary>
        [HttpDelete]
        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> DeleteAll()
        {
            try
            {
                await _repository.DeleteAllAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.UserModule;
using Api.DTOs;
using Api.Main;
using Api.Security;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize] // All endpoints require authentication
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _repository;

        public UserController(IUserRepository repository)
        {
            _repository = repository;
        }

        private static UserResponse ToResponse(User entity)
        {
            return new UserResponse
            {
                UserId = entity.UserId,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                Email = entity.Email,
                ContactNumber = entity.ContactNumber,
                Username = entity.Username,
                RoleId = entity.RoleId,
                CreatedAt = entity.CreatedAt
            };
        }

        public sealed class SearchByFilterRequest
        {
            public int? userId { get; set; }
        }

        private static Dictionary<string, object?> BuildFilterMap(SearchByFilterRequest request)
        {
            var filters = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            if (request.userId != null)
            {
                filters["userId"] = request.userId;
            }
            return filters;
        }

        /// <summary>
        /// Get all users (UserAccess only)
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll()
        {
            try
            {
                var result = await _repository.GetAllAsync();
                return Ok(result.Select(ToResponse).ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get user by ID (UserAccess only)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<UserResponse>> GetById(int id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                    return NotFound(new { message = "User not found" });
                return Ok(ToResponse(entity));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Dynamic exact-match filters for user (UserAccess only)
        /// </summary>
        [HttpGet("search-by-filter-ematch")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<IEnumerable<UserResponse>>> SearchByFilterExact([FromQuery] SearchByFilterRequest request)
        {
            try
            {
                IReadOnlyDictionary<string, object?> filters = BuildFilterMap(request);
                if (filters.Count == 0)
                {
                    return BadRequest(new { message = "At least one filter must be provided." });
                }

                var result = await _repository.GetFilteredExactAsync(filters);
                return Ok(result.Select(ToResponse).ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Dynamic partial-match filters for user (UserAccess only)
        /// </summary>
        [HttpGet("search-by-filter-lmatch")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<IEnumerable<UserResponse>>> SearchByFilterLike([FromQuery] SearchByFilterRequest request)
        {
            try
            {
                IReadOnlyDictionary<string, object?> filters = BuildFilterMap(request);
                if (filters.Count == 0)
                {
                    return BadRequest(new { message = "At least one filter must be provided." });
                }

                var result = await _repository.GetFilteredLikeAsync(filters);
                return Ok(result.Select(ToResponse).ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Search all users (UserAccess only)
        /// </summary>
        [HttpGet("search-all")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<IEnumerable<UserResponse>>> SearchAll([FromQuery] string query)
        {
            try
            {
                var result = await _repository.SearchAsyncAll(query);
                return Ok(result.Select(ToResponse).ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get paginated users (UserAccess only)
        /// </summary>
        [HttpGet("paginated")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult> GetPaginated(int pageNumber = 1, int pageSize = 25)
        {
            try
            {
                var pagedResult = await _repository.GetPaginatedAsync(pageNumber, pageSize);
                var response = new PaginationModel<UserResponse>
                {
                    Items = pagedResult.Items.Select(ToResponse).ToList(),
                    TotalCount = pagedResult.TotalCount,
                    PageSize = pagedResult.PageSize,
                    CurrentPage = pagedResult.CurrentPage
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Create new user (UserAccess only)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "UserAccess")]
        public async Task<IActionResult> Create([FromBody] RegisterRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                    return BadRequest(new { message = "Username and Password are required" });

                var entity = new User
                {
                    Username      = request.Username,
                    PasswordHash  = PasswordHasher.Hash(request.Password),
                    RoleId        = request.RoleId,
                    FirstName     = request.FirstName,
                    LastName      = request.LastName,
                    Email         = request.Email,
                    ContactNumber = request.ContactNumber,
                    CreatedAt     = DateTime.UtcNow,
                };

                await _repository.AddAsync(entity);
                return Ok(new { message = "User created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update user (UserAccess only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "UserAccess")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username))
                    return BadRequest(new { message = "Username is required" });

                var existingEntity = await _repository.GetByIdAsync(id);
                if (existingEntity == null)
                    return NotFound(new { message = "User not found" });

                existingEntity.FirstName     = request.FirstName;
                existingEntity.LastName      = request.LastName;
                existingEntity.Email         = request.Email;
                existingEntity.ContactNumber = request.ContactNumber;
                existingEntity.Username      = request.Username;
                existingEntity.RoleId        = request.RoleId;
                if (!string.IsNullOrWhiteSpace(request.NewPassword))
                    existingEntity.PasswordHash = PasswordHasher.Hash(request.NewPassword);

                await _repository.UpdateAsync(existingEntity);
                return Ok(new { message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete user (UserAccess only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "UserAccess")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existingEntity = await _repository.GetByIdAsync(id);
                if (existingEntity == null)
                    return NotFound(new { message = "User not found" });

                await _repository.DeleteAsync(id);
                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete all users (UserAccess only)
        /// </summary>
        [HttpDelete]
        [Authorize(Policy = "UserAccess")]
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

        /// <summary>
        /// Bulk upload users (UserAccess only)
        /// </summary>
        [HttpPost("bulk-upload")]
        [Authorize(Policy = "UserAccess")]
        public async Task<IActionResult> BulkUpload([FromBody] List<RegisterRequest> dataList)
        {
            try
            {
                if (dataList == null || dataList.Count == 0)
                    return BadRequest("Data list cannot be null or empty.");

                if (dataList.Any(item => string.IsNullOrWhiteSpace(item.Username)))
                    return BadRequest(new { message = "One or more records have empty Username." });
                if (dataList.Any(item => string.IsNullOrWhiteSpace(item.Password)))
                    return BadRequest(new { message = "One or more records have empty Password." });

                var entities = dataList.Select(item => new User
                {
                    Username      = item.Username,
                    PasswordHash  = PasswordHasher.Hash(item.Password),
                    RoleId        = item.RoleId,
                    FirstName     = item.FirstName,
                    LastName      = item.LastName,
                    Email         = item.Email,
                    ContactNumber = item.ContactNumber,
                    CreatedAt     = DateTime.UtcNow,
                }).ToList();

                await _repository.BulkUploadAsync(entities);
                return Ok(new { message = $"Successfully uploaded {entities.Count} user records." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
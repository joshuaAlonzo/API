using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.CategoriesModule;
using Api.Main;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _repository;

        public CategoryController(ICategoryRepository repository)
        {
            _repository = repository;
        }

        // ---------------------------------------------------------------
        //  Request shapes
        // ---------------------------------------------------------------

        public sealed class CreateCategoryRequest
        {
            public string CategoryName { get; set; } = string.Empty;
        }

        public sealed class UpdateCategoryRequest
        {
            public string CategoryName { get; set; } = string.Empty;
        }

        // ---------------------------------------------------------------
        //  GET endpoints
        // ---------------------------------------------------------------

        /// <summary>Get all categories — UserAccess.</summary>
        [HttpGet]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<IEnumerable<Category>>> GetAll()
        {
            try
            {
                return Ok(await _repository.GetAllAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Get a category by ID — UserAccess.</summary>
        [HttpGet("{id:int}")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<Category>> GetById(int id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                    return NotFound(new { message = "Category not found." });
                return Ok(entity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Get a category by exact name — UserAccess.</summary>
        [HttpGet("by-name")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<Category>> GetByName([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return BadRequest(new { message = "Name query parameter is required." });

                var entity = await _repository.GetByNameAsync(name);
                if (entity == null)
                    return NotFound(new { message = "Category not found." });
                return Ok(entity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Search categories by partial name — UserAccess.</summary>
        [HttpGet("search")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<IEnumerable<Category>>> Search([FromQuery] string query)
        {
            try
            {
                return Ok(await _repository.SearchAsync(query ?? string.Empty));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Paginated list of categories — UserAccess.</summary>
        [HttpGet("paginated")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult> GetPaginated(int pageNumber = 1, int pageSize = 25)
        {
            try
            {
                var paged = await _repository.GetPaginatedAsync(pageNumber, pageSize);
                return Ok(new PaginationModel<Category>
                {
                    Items       = paged.Items,
                    TotalCount  = paged.TotalCount,
                    PageSize    = paged.PageSize,
                    CurrentPage = paged.CurrentPage,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ---------------------------------------------------------------
        //  POST endpoint
        // ---------------------------------------------------------------

        /// <summary>Create a new category — AdminAccess.</summary>
        [HttpPost]
        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.CategoryName))
                    return BadRequest(new { message = "CategoryName is required." });

                // Duplicate check
                var existing = await _repository.GetByNameAsync(request.CategoryName);
                if (existing != null)
                    return Conflict(new { message = $"Category '{request.CategoryName}' already exists." });

                await _repository.AddAsync(new Category { CategoryName = request.CategoryName.Trim() });
                return Ok(new { message = "Category created successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ---------------------------------------------------------------
        //  PUT endpoint
        // ---------------------------------------------------------------

        /// <summary>Update a category name — AdminAccess.</summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.CategoryName))
                    return BadRequest(new { message = "CategoryName is required." });

                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { message = "Category not found." });

                // Duplicate name check (ignore self)
                var duplicate = await _repository.GetByNameAsync(request.CategoryName);
                if (duplicate != null && duplicate.CategoryId != id)
                    return Conflict(new { message = $"Category '{request.CategoryName}' already exists." });

                existing.CategoryName = request.CategoryName.Trim();
                await _repository.UpdateAsync(existing);
                return Ok(new { message = "Category updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ---------------------------------------------------------------
        //  DELETE endpoints
        // ---------------------------------------------------------------

        /// <summary>Delete a category by ID — AdminAccess.</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { message = "Category not found." });

                await _repository.DeleteAsync(id);
                return Ok(new { message = "Category deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Delete all categories — AdminAccess.</summary>
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

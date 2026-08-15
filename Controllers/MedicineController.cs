using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.MedicineModule;
using Api.Main;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class MedicineController : ControllerBase
    {
        private readonly IMedicineRepository _repository;

        public MedicineController(IMedicineRepository repository)
        {
            _repository = repository;
        }

        // ---------------------------------------------------------------
        //  Request shapes
        // ---------------------------------------------------------------

        public sealed class CreateMedicineRequest
        {
            public string BrandName { get; set; } = string.Empty;
            public string GenericName { get; set; } = string.Empty;
            public int CategoryId { get; set; }
            public string? Description { get; set; }
            public string? Dosage { get; set; }
            public string? Manufacturer { get; set; }
            public double Price { get; set; }
            public int StockQty { get; set; }
            public string? Image { get; set; }
            public string Status { get; set; } = "Available";
        }

        public sealed class UpdateMedicineRequest
        {
            public string BrandName { get; set; } = string.Empty;
            public string GenericName { get; set; } = string.Empty;
            public int CategoryId { get; set; }
            public string? Description { get; set; }
            public string? Dosage { get; set; }
            public string? Manufacturer { get; set; }
            public double Price { get; set; }
            public int StockQty { get; set; }
            public string? Image { get; set; }
            public string Status { get; set; } = "Available";
        }

        // ---------------------------------------------------------------
        //  GET endpoints
        // ---------------------------------------------------------------

        /// <summary>Get all medicines — UserAccess.</summary>
        [HttpGet]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<IEnumerable<Medicine>>> GetAll()
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

        /// <summary>Get a medicine by ID — UserAccess.</summary>
        [HttpGet("{id:int}")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<Medicine>> GetById(int id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                    return NotFound(new { message = "Medicine not found." });
                return Ok(entity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Get medicines by category ID — UserAccess.</summary>
        [HttpGet("category/{categoryId:int}")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<IEnumerable<Medicine>>> GetByCategoryId(int categoryId)
        {
            try
            {
                return Ok(await _repository.GetByCategoryIdAsync(categoryId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Search medicines by brand/generic name or description — UserAccess.</summary>
        [HttpGet("search")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<IEnumerable<Medicine>>> Search([FromQuery] string query)
        {
            try
            {
                return Ok(await _repository.SearchAsync(query));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Get paginated list of medicines — UserAccess.</summary>
        [HttpGet("paginated")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<PaginationModel<Medicine>>> GetPaginated(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? categoryId = null,
            [FromQuery] string? status = null)
        {
            try
            {
                return Ok(await _repository.GetPaginatedAsync(pageNumber, pageSize, categoryId, status));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ---------------------------------------------------------------
        //  POST / PUT / DELETE endpoints
        // ---------------------------------------------------------------

        /// <summary>Create a medicine — Admin/Mod Access.</summary>
        [HttpPost]
        [Authorize(Policy = "ModAccess")]
        public async Task<ActionResult<Medicine>> Create([FromBody] CreateMedicineRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.BrandName) || string.IsNullOrWhiteSpace(request.GenericName))
                    return BadRequest(new { message = "Brand name and generic name are required." });

                var entity = new Medicine
                {
                    BrandName    = request.BrandName,
                    GenericName  = request.GenericName,
                    CategoryId   = request.CategoryId,
                    Description  = request.Description,
                    Dosage       = request.Dosage,
                    Manufacturer = request.Manufacturer,
                    Price        = request.Price,
                    StockQty     = request.StockQty,
                    Image        = request.Image,
                    Status       = string.IsNullOrWhiteSpace(request.Status) ? "Available" : request.Status,
                    CreatedAt    = DateTime.UtcNow,
                    UpdatedAt    = DateTime.UtcNow
                };

                await _repository.AddAsync(entity);
                return CreatedAtAction(nameof(GetById), new { id = entity.MedicineId }, entity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Update a medicine — Admin/Mod Access.</summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = "ModAccess")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateMedicineRequest request)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { message = "Medicine not found." });

                existing.BrandName    = request.BrandName;
                existing.GenericName  = request.GenericName;
                existing.CategoryId   = request.CategoryId;
                existing.Description  = request.Description;
                existing.Dosage       = request.Dosage;
                existing.Manufacturer = request.Manufacturer;
                existing.Price        = request.Price;
                existing.StockQty     = request.StockQty;
                existing.Image        = request.Image;
                existing.Status       = string.IsNullOrWhiteSpace(request.Status) ? "Available" : request.Status;
                existing.UpdatedAt    = DateTime.UtcNow;

                await _repository.UpdateAsync(existing);
                return Ok(new { message = "Medicine updated successfully.", medicine = existing });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Delete a medicine — AdminAccess.</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminAccess")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { message = "Medicine not found." });

                await _repository.DeleteAsync(id);
                return Ok(new { message = "Medicine deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}

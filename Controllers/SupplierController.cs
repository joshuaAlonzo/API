using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.SuppliersModule;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize] // Require auth for all methods by default
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierRepository _repository;

        public SupplierController(ISupplierRepository repository)
        {
            _repository = repository;
        }

        public sealed class CreateSupplierRequest
        {
            public string SupplierName { get; set; } = string.Empty;
            public string? ContactPerson { get; set; }
            public string? ContactNumber { get; set; }
            public string? Email { get; set; }
            public string? Address { get; set; }
            public string Status { get; set; } = "Active";
        }

        [HttpGet]
        [Authorize(Policy = "ModAccess")]
        public async Task<ActionResult<IEnumerable<Supplier>>> GetAll()
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

        [HttpGet("{id:int}")]
        [Authorize(Policy = "ModAccess")]
        public async Task<ActionResult<Supplier>> GetById(int id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null) return NotFound(new { message = "Supplier not found." });
                return Ok(entity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> Create([FromBody] CreateSupplierRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.SupplierName))
                    return BadRequest(new { message = "SupplierName is required." });

                var supplier = new Supplier
                {
                    SupplierName = request.SupplierName.Trim(),
                    ContactPerson = request.ContactPerson,
                    ContactNumber = request.ContactNumber,
                    Email = request.Email,
                    Address = request.Address,
                    Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status
                };

                int id = await _repository.AddAsync(supplier);
                return CreatedAtAction(nameof(GetById), new { id = id }, new { message = "Supplier created.", supplierId = id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateSupplierRequest request)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(id);
                if (existing == null) return NotFound(new { message = "Supplier not found." });

                existing.SupplierName = string.IsNullOrWhiteSpace(request.SupplierName) ? existing.SupplierName : request.SupplierName.Trim();
                existing.ContactPerson = request.ContactPerson;
                existing.ContactNumber = request.ContactNumber;
                existing.Email = request.Email;
                existing.Address = request.Address;
                if (!string.IsNullOrWhiteSpace(request.Status)) existing.Status = request.Status;

                await _repository.UpdateAsync(existing);
                return Ok(new { message = "Supplier updated." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(id);
                if (existing == null) return NotFound(new { message = "Supplier not found." });

                await _repository.DeleteAsync(id);
                return Ok(new { message = "Supplier deleted." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}

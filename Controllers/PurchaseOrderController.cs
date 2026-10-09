using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.PurchaseOrderModule;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class PurchaseOrderController : ControllerBase
    {
        private readonly IPurchaseOrderRepository _repository;

        public PurchaseOrderController(IPurchaseOrderRepository repository)
        {
            _repository = repository;
        }

        public sealed class PurchaseOrderRequest
        {
            public string PurchaseOrderNumber { get; set; } = string.Empty;
            public int SupplierId { get; set; }
            public string Status { get; set; } = "Draft";
            public DateTime? OrderedAt { get; set; }
            public DateTime? ExpectedAt { get; set; }
            public DateTime? ReceivedAt { get; set; }
            public int? CreatedByUserId { get; set; }
            public string? Notes { get; set; }
        }

        [HttpGet]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<IEnumerable<PurchaseOrder>>> GetAll()
        {
            return Ok(await _repository.GetAllAsync());
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<PurchaseOrder>> GetById(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return NotFound(new { message = "PO not found." });
            return Ok(entity);
        }

        [HttpPost]
        [Authorize(Policy = "ModAccess")]
        public async Task<ActionResult<PurchaseOrder>> Create([FromBody] PurchaseOrderRequest request)
        {
            var entity = new PurchaseOrder
            {
                PurchaseOrderNumber = request.PurchaseOrderNumber,
                SupplierId = request.SupplierId,
                Status = request.Status,
                OrderedAt = request.OrderedAt,
                ExpectedAt = request.ExpectedAt,
                ReceivedAt = request.ReceivedAt,
                CreatedByUserId = request.CreatedByUserId,
                Notes = request.Notes
            };
            await _repository.AddAsync(entity);
            return Ok(new { message = "PO created successfully." });
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = "ModAccess")]
        public async Task<ActionResult> Update(int id, [FromBody] PurchaseOrderRequest request)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound(new { message = "PO not found." });

            existing.PurchaseOrderNumber = request.PurchaseOrderNumber;
            existing.SupplierId = request.SupplierId;
            existing.Status = request.Status;
            existing.OrderedAt = request.OrderedAt;
            existing.ExpectedAt = request.ExpectedAt;
            existing.ReceivedAt = request.ReceivedAt;
            existing.CreatedByUserId = request.CreatedByUserId;
            existing.Notes = request.Notes;

            await _repository.UpdateAsync(existing);
            return Ok(new { message = "PO updated successfully." });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminAccess")]
        public async Task<ActionResult> Delete(int id)
        {
            await _repository.DeleteAsync(id);
            return Ok(new { message = "PO deleted successfully." });
        }
    }
}

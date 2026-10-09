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
    public class PurchaseOrderItemController : ControllerBase
    {
        private readonly IPurchaseOrderItemRepository _repository;

        public PurchaseOrderItemController(IPurchaseOrderItemRepository repository)
        {
            _repository = repository;
        }

        public sealed class PurchaseOrderItemRequest
        {
            public int PurchaseOrderId { get; set; }
            public int MedicineId { get; set; }
            public int QuantityOrdered { get; set; }
            public int QuantityReceived { get; set; }
            public double UnitCost { get; set; }
            public string? Notes { get; set; }
        }

        [HttpGet]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<IEnumerable<PurchaseOrderItem>>> GetAll()
        {
            return Ok(await _repository.GetAllAsync());
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<PurchaseOrderItem>> GetById(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return NotFound(new { message = "PO Item not found." });
            return Ok(entity);
        }

        [HttpGet("purchaseorder/{poId:int}")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<IEnumerable<PurchaseOrderItem>>> GetByPurchaseOrderId(int poId)
        {
            return Ok(await _repository.GetByPurchaseOrderIdAsync(poId));
        }

        [HttpPost]
        [Authorize(Policy = "ModAccess")]
        public async Task<ActionResult<PurchaseOrderItem>> Create([FromBody] PurchaseOrderItemRequest request)
        {
            var entity = new PurchaseOrderItem
            {
                PurchaseOrderId = request.PurchaseOrderId,
                MedicineId = request.MedicineId,
                QuantityOrdered = request.QuantityOrdered,
                QuantityReceived = request.QuantityReceived,
                UnitCost = request.UnitCost,
                Notes = request.Notes
            };
            await _repository.AddAsync(entity);
            return Ok(new { message = "PO Item created successfully." });
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = "ModAccess")]
        public async Task<ActionResult> Update(int id, [FromBody] PurchaseOrderItemRequest request)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound(new { message = "PO Item not found." });

            existing.PurchaseOrderId = request.PurchaseOrderId;
            existing.MedicineId = request.MedicineId;
            existing.QuantityOrdered = request.QuantityOrdered;
            existing.QuantityReceived = request.QuantityReceived;
            existing.UnitCost = request.UnitCost;
            existing.Notes = request.Notes;

            await _repository.UpdateAsync(existing);
            return Ok(new { message = "PO Item updated successfully." });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminAccess")]
        public async Task<ActionResult> Delete(int id)
        {
            await _repository.DeleteAsync(id);
            return Ok(new { message = "PO Item deleted successfully." });
        }
    }
}

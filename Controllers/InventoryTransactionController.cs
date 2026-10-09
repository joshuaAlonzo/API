using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.InventoryTransactionModule;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class InventoryTransactionController : ControllerBase
    {
        private readonly IInventoryTransactionRepository _repository;

        public InventoryTransactionController(IInventoryTransactionRepository repository)
        {
            _repository = repository;
        }

        public sealed class InventoryTransactionRequest
        {
            public int MedicineId { get; set; }
            public int? BatchId { get; set; }
            public string TransactionType { get; set; } = string.Empty;
            public int QuantityChange { get; set; }
            public double? UnitCost { get; set; }
            public int? RelatedOrderId { get; set; }
            public int? PurchaseOrderItemId { get; set; }
            public int? PerformedByUserId { get; set; }
            public string Reason { get; set; } = string.Empty;
            public string? Notes { get; set; }
        }

        [HttpGet]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<IEnumerable<InventoryTransaction>>> GetAll()
        {
            return Ok(await _repository.GetAllAsync());
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<InventoryTransaction>> GetById(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return NotFound(new { message = "Transaction not found." });
            return Ok(entity);
        }

        [HttpGet("medicine/{medicineId:int}")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<IEnumerable<InventoryTransaction>>> GetByMedicineId(int medicineId)
        {
            return Ok(await _repository.GetByMedicineIdAsync(medicineId));
        }

        [HttpPost]
        [Authorize(Policy = "ModAccess")]
        public async Task<ActionResult<InventoryTransaction>> Create([FromBody] InventoryTransactionRequest request)
        {
            var entity = new InventoryTransaction
            {
                MedicineId = request.MedicineId,
                BatchId = request.BatchId,
                TransactionType = request.TransactionType,
                QuantityChange = request.QuantityChange,
                UnitCost = request.UnitCost,
                RelatedOrderId = request.RelatedOrderId,
                PurchaseOrderItemId = request.PurchaseOrderItemId,
                PerformedByUserId = request.PerformedByUserId,
                Reason = request.Reason,
                Notes = request.Notes
            };
            await _repository.AddAsync(entity);
            return Ok(new { message = "Transaction created successfully." });
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = "ModAccess")]
        public async Task<ActionResult> Update(int id, [FromBody] InventoryTransactionRequest request)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound(new { message = "Transaction not found." });

            existing.MedicineId = request.MedicineId;
            existing.BatchId = request.BatchId;
            existing.TransactionType = request.TransactionType;
            existing.QuantityChange = request.QuantityChange;
            existing.UnitCost = request.UnitCost;
            existing.RelatedOrderId = request.RelatedOrderId;
            existing.PurchaseOrderItemId = request.PurchaseOrderItemId;
            existing.PerformedByUserId = request.PerformedByUserId;
            existing.Reason = request.Reason;
            existing.Notes = request.Notes;

            await _repository.UpdateAsync(existing);
            return Ok(new { message = "Transaction updated successfully." });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminAccess")]
        public async Task<ActionResult> Delete(int id)
        {
            await _repository.DeleteAsync(id);
            return Ok(new { message = "Transaction deleted successfully." });
        }
    }
}

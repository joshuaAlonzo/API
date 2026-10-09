using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.MedicineBatchModule;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class MedicineBatchController : ControllerBase
    {
        private readonly IMedicineBatchRepository _repository;

        public MedicineBatchController(IMedicineBatchRepository repository)
        {
            _repository = repository;
        }

        public sealed class MedicineBatchRequest
        {
            public int MedicineId { get; set; }
            public int? SupplierId { get; set; }
            public string LotNumber { get; set; } = string.Empty;
            public DateTime ExpirationDate { get; set; }
            public int QuantityReceived { get; set; }
            public int QuantityRemaining { get; set; }
            public double UnitCost { get; set; }
            public string BatchStatus { get; set; } = "Active";
            public string? Notes { get; set; }
        }

        [HttpGet]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<IEnumerable<MedicineBatch>>> GetAll()
        {
            return Ok(await _repository.GetAllAsync());
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<MedicineBatch>> GetById(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return NotFound(new { message = "Batch not found." });
            return Ok(entity);
        }

        [HttpPost]
        [Authorize(Policy = "ModAccess")]
        public async Task<ActionResult<MedicineBatch>> Create([FromBody] MedicineBatchRequest request)
        {
            var entity = new MedicineBatch
            {
                MedicineId = request.MedicineId,
                SupplierId = request.SupplierId,
                LotNumber = request.LotNumber,
                ExpirationDate = request.ExpirationDate,
                QuantityReceived = request.QuantityReceived,
                QuantityRemaining = request.QuantityRemaining,
                UnitCost = request.UnitCost,
                BatchStatus = request.BatchStatus,
                Notes = request.Notes
            };
            await _repository.AddAsync(entity);
            return Ok(new { message = "Batch created successfully." });
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = "ModAccess")]
        public async Task<ActionResult> Update(int id, [FromBody] MedicineBatchRequest request)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound(new { message = "Batch not found." });

            existing.MedicineId = request.MedicineId;
            existing.SupplierId = request.SupplierId;
            existing.LotNumber = request.LotNumber;
            existing.ExpirationDate = request.ExpirationDate;
            existing.QuantityReceived = request.QuantityReceived;
            existing.QuantityRemaining = request.QuantityRemaining;
            existing.UnitCost = request.UnitCost;
            existing.BatchStatus = request.BatchStatus;
            existing.Notes = request.Notes;

            await _repository.UpdateAsync(existing);
            return Ok(new { message = "Batch updated successfully." });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminAccess")]
        public async Task<ActionResult> Delete(int id)
        {
            await _repository.DeleteAsync(id);
            return Ok(new { message = "Batch deleted successfully." });
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.OrderModule;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class OrderStatusHistoryController : ControllerBase
    {
        private readonly IOrderStatusHistoryRepository _repository;

        public OrderStatusHistoryController(IOrderStatusHistoryRepository repository)
        {
            _repository = repository;
        }

        public sealed class OrderStatusHistoryRequest
        {
            public int? OrderId { get; set; }
            public int OrderIdSnapshot { get; set; }
            public string? OldStatus { get; set; }
            public string NewStatus { get; set; } = string.Empty;
            public int? ChangedByUserId { get; set; }
            public string? Notes { get; set; }
        }

        [HttpGet]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<IEnumerable<OrderStatusHistory>>> GetAll()
        {
            return Ok(await _repository.GetAllAsync());
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<OrderStatusHistory>> GetById(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return NotFound(new { message = "History not found." });
            return Ok(entity);
        }

        [HttpGet("order/{orderId:int}")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<IEnumerable<OrderStatusHistory>>> GetByOrderId(int orderId)
        {
            return Ok(await _repository.GetByOrderIdAsync(orderId));
        }

        [HttpPost]
        [Authorize(Policy = "ModAccess")]
        public async Task<ActionResult<OrderStatusHistory>> Create([FromBody] OrderStatusHistoryRequest request)
        {
            var entity = new OrderStatusHistory
            {
                OrderId = request.OrderId,
                OrderIdSnapshot = request.OrderIdSnapshot,
                OldStatus = request.OldStatus,
                NewStatus = request.NewStatus,
                ChangedByUserId = request.ChangedByUserId,
                Notes = request.Notes
            };
            await _repository.AddAsync(entity);
            return Ok(new { message = "History created successfully." });
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = "ModAccess")]
        public async Task<ActionResult> Update(int id, [FromBody] OrderStatusHistoryRequest request)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound(new { message = "History not found." });

            existing.OrderId = request.OrderId;
            existing.OrderIdSnapshot = request.OrderIdSnapshot;
            existing.OldStatus = request.OldStatus;
            existing.NewStatus = request.NewStatus;
            existing.ChangedByUserId = request.ChangedByUserId;
            existing.Notes = request.Notes;

            await _repository.UpdateAsync(existing);
            return Ok(new { message = "History updated successfully." });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminAccess")]
        public async Task<ActionResult> Delete(int id)
        {
            await _repository.DeleteAsync(id);
            return Ok(new { message = "History deleted successfully." });
        }
    }
}

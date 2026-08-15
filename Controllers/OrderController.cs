using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Api.OrderModule;
using Api.Main;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _repository;

        public OrderController(IOrderRepository repository)
        {
            _repository = repository;
        }

        // ---------------------------------------------------------------
        //  Request DTOs
        // ---------------------------------------------------------------

        public sealed class CheckoutRequest
        {
            public int? UserId { get; set; }
            public string? PaymentMethod { get; set; }
            public string? DeliveryAddress { get; set; }
        }

        public sealed class CreateOrderItemRequest
        {
            public int MedicineId { get; set; }
            public int Quantity { get; set; }
            public double UnitPrice { get; set; }
        }

        public sealed class CreateOrderRequest
        {
            public int UserId { get; set; }
            public string? PaymentMethod { get; set; }
            public string? DeliveryAddress { get; set; }
            public List<CreateOrderItemRequest> Items { get; set; } = new List<CreateOrderItemRequest>();
        }

        public sealed class UpdateOrderStatusRequest
        {
            public string Status { get; set; } = "Pending";
        }

        // ---------------------------------------------------------------
        //  GET Endpoints
        // ---------------------------------------------------------------

        /// <summary>Get all orders — Admin/Mod Access.</summary>
        [HttpGet]
        [Authorize(Policy = "ModAccess")]
        public async Task<ActionResult<IEnumerable<Order>>> GetAll()
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

        /// <summary>Get order by ID — UserAccess.</summary>
        [HttpGet("{id:int}")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<Order>> GetById(int id)
        {
            try
            {
                var order = await _repository.GetByIdAsync(id);
                if (order == null)
                    return NotFound(new { message = "Order not found." });

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Get orders by User ID — UserAccess.</summary>
        [HttpGet("user/{userId:int}")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<IEnumerable<Order>>> GetByUserId(int userId)
        {
            try
            {
                return Ok(await _repository.GetByUserIdAsync(userId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Get paginated orders — UserAccess.</summary>
        [HttpGet("paginated")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<PaginationModel<Order>>> GetPaginated(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? userId = null,
            [FromQuery] string? status = null)
        {
            try
            {
                return Ok(await _repository.GetPaginatedAsync(pageNumber, pageSize, userId, status));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ---------------------------------------------------------------
        //  POST / PUT / DELETE Endpoints
        // ---------------------------------------------------------------

        /// <summary>Checkout user's active cart into an order — UserAccess.</summary>
        [HttpPost("checkout")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<Order>> Checkout([FromBody] CheckoutRequest request)
        {
            try
            {
                int targetUserId = request.UserId ?? GetUserIdFromClaims();
                if (targetUserId <= 0)
                    return BadRequest(new { message = "User ID is required for checkout." });

                var order = await _repository.CheckoutFromCartAsync(targetUserId, request.PaymentMethod, request.DeliveryAddress);
                return CreatedAtAction(nameof(GetById), new { id = order.OrderId }, order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Create a new order explicitly — UserAccess.</summary>
        [HttpPost]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<Order>> Create([FromBody] CreateOrderRequest request)
        {
            try
            {
                if (request.UserId <= 0)
                    return BadRequest(new { message = "User ID is required." });
                if (request.Items == null || request.Items.Count == 0)
                    return BadRequest(new { message = "Order must contain at least one item." });

                var items = request.Items.ConvertAll(i => new OrderItem
                {
                    MedicineId = i.MedicineId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Subtotal = i.UnitPrice * i.Quantity
                });

                var order = await _repository.CreateOrderAsync(request.UserId, request.PaymentMethod, request.DeliveryAddress, items);
                return CreatedAtAction(nameof(GetById), new { id = order.OrderId }, order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Update order status (e.g. Pending -> Completed / Cancelled) — Admin/Mod Access.</summary>
        [HttpPut("{id:int}/status")]
        [Authorize(Policy = "ModAccess")]
        public async Task<ActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(id, includeItems: false);
                if (existing == null)
                    return NotFound(new { message = "Order not found." });

                await _repository.UpdateStatusAsync(id, request.Status);
                return Ok(new { message = "Order status updated successfully.", orderId = id, status = request.Status });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Delete an order — AdminAccess.</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminAccess")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(id, includeItems: false);
                if (existing == null)
                    return NotFound(new { message = "Order not found." });

                await _repository.DeleteAsync(id);
                return Ok(new { message = "Order deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        private int GetUserIdFromClaims()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return (claim != null && int.TryParse(claim.Value, out int id)) ? id : 0;
        }
    }
}

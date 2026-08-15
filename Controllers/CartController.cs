using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.CartModule;
using Api.Main;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _repository;

        public CartController(ICartRepository repository)
        {
            _repository = repository;
        }

        // ---------------------------------------------------------------
        //  Request shapes
        // ---------------------------------------------------------------

        public sealed class AddToCartRequest
        {
            public int    UserId     { get; set; }
            public int    MedicineId { get; set; }
            public int    Quantity   { get; set; }
            public double Subtotal   { get; set; }
        }

        public sealed class UpdateCartRequest
        {
            public int    Quantity { get; set; }
            public double Subtotal { get; set; }
        }

        public sealed class SearchByFilterRequest
        {
            public int? CartId     { get; set; }
            public int? UserId     { get; set; }
            public int? MedicineId { get; set; }
        }

        private static Dictionary<string, object?> BuildFilterMap(SearchByFilterRequest request)
        {
            var filters = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            if (request.CartId     != null) filters["cart_id"]     = request.CartId;
            if (request.UserId     != null) filters["user_id"]     = request.UserId;
            if (request.MedicineId != null) filters["medicine_id"] = request.MedicineId;
            return filters;
        }

        // ---------------------------------------------------------------
        //  GET endpoints
        // ---------------------------------------------------------------

        /// <summary>Get all cart items — AdminAccess only.</summary>
        [HttpGet]
        [Authorize(Policy = "AdminAccess")]
        public async Task<ActionResult<IEnumerable<Cart>>> GetAll()
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

        /// <summary>Get a single cart item by ID — UserAccess.</summary>
        [HttpGet("{id:int}")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<Cart>> GetById(int id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                    return NotFound(new { message = "Cart item not found." });
                return Ok(entity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Get all cart items for a specific user — UserAccess.</summary>
        [HttpGet("user/{userId:int}")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<IEnumerable<Cart>>> GetByUserId(int userId)
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

        /// <summary>Get a specific cart item by user + medicine — UserAccess.</summary>
        [HttpGet("user/{userId:int}/medicine/{medicineId:int}")]
        [Authorize(Policy = "UserAccess")]
        public async Task<ActionResult<Cart>> GetByUserAndMedicine(int userId, int medicineId)
        {
            try
            {
                var entity = await _repository.GetByUserAndMedicineAsync(userId, medicineId);
                if (entity == null)
                    return NotFound(new { message = "Cart item not found for this user and medicine." });
                return Ok(entity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Exact-match filter — AdminAccess.</summary>
        [HttpGet("search-by-filter-ematch")]
        [Authorize(Policy = "AdminAccess")]
        public async Task<ActionResult<IEnumerable<Cart>>> SearchByFilterExact(
            [FromQuery] SearchByFilterRequest request)
        {
            try
            {
                IReadOnlyDictionary<string, object?> filters = BuildFilterMap(request);
                if (filters.Count == 0)
                    return BadRequest(new { message = "At least one filter must be provided." });

                return Ok(await _repository.GetFilteredExactAsync(filters));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Paginated list of cart items — AdminAccess.</summary>
        [HttpGet("paginated")]
        [Authorize(Policy = "AdminAccess")]
        public async Task<ActionResult> GetPaginated(int pageNumber = 1, int pageSize = 25)
        {
            try
            {
                var paged = await _repository.GetPaginatedAsync(pageNumber, pageSize);
                return Ok(new PaginationModel<Cart>
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

        /// <summary>
        /// Add a medicine to the cart.
        /// If the medicine already exists in the user's cart, the quantity and subtotal are updated.
        /// — UserAccess.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "UserAccess")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            try
            {
                if (request.UserId <= 0)
                    return BadRequest(new { message = "A valid UserId is required." });
                if (request.MedicineId <= 0)
                    return BadRequest(new { message = "A valid MedicineId is required." });
                if (request.Quantity <= 0)
                    return BadRequest(new { message = "Quantity must be greater than zero." });

                // Upsert: merge with existing item if present
                var existing = await _repository.GetByUserAndMedicineAsync(request.UserId, request.MedicineId);
                if (existing != null)
                {
                    existing.Quantity += request.Quantity;
                    existing.Subtotal += request.Subtotal;
                    await _repository.UpdateAsync(existing);
                    return Ok(new { message = "Cart item quantity updated." });
                }

                var entity = new Cart
                {
                    UserId     = request.UserId,
                    MedicineId = request.MedicineId,
                    Quantity   = request.Quantity,
                    Subtotal   = request.Subtotal,
                    AddedAt    = DateTime.UtcNow,
                };

                await _repository.AddAsync(entity);
                return Ok(new { message = "Item added to cart." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ---------------------------------------------------------------
        //  PUT endpoint
        // ---------------------------------------------------------------

        /// <summary>Update quantity / subtotal of a cart item — UserAccess.</summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = "UserAccess")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCartRequest request)
        {
            try
            {
                if (request.Quantity <= 0)
                    return BadRequest(new { message = "Quantity must be greater than zero." });

                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { message = "Cart item not found." });

                existing.Quantity = request.Quantity;
                existing.Subtotal = request.Subtotal;
                await _repository.UpdateAsync(existing);
                return Ok(new { message = "Cart item updated." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ---------------------------------------------------------------
        //  DELETE endpoints
        // ---------------------------------------------------------------

        /// <summary>Remove a single cart item by ID — UserAccess.</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "UserAccess")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { message = "Cart item not found." });

                await _repository.DeleteAsync(id);
                return Ok(new { message = "Cart item removed." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Remove a specific medicine from a user's cart — UserAccess.</summary>
        [HttpDelete("user/{userId:int}/medicine/{medicineId:int}")]
        [Authorize(Policy = "UserAccess")]
        public async Task<IActionResult> DeleteByUserAndMedicine(int userId, int medicineId)
        {
            try
            {
                await _repository.DeleteByUserAndMedicineAsync(userId, medicineId);
                return Ok(new { message = "Item removed from cart." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Clear all items in a user's cart — UserAccess.</summary>
        [HttpDelete("user/{userId:int}/clear")]
        [Authorize(Policy = "UserAccess")]
        public async Task<IActionResult> ClearCart(int userId)
        {
            try
            {
                await _repository.ClearCartAsync(userId);
                return Ok(new { message = $"Cart cleared for user {userId}." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Delete all cart records — AdminAccess.</summary>
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

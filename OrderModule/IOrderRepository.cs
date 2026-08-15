using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Main;

namespace Api.OrderModule
{
    public interface IOrderRepository
    {
        /// <summary>Returns all orders.</summary>
        Task<IEnumerable<Order>> GetAllAsync();

        /// <summary>Returns a single order by primary key, optionally including order items.</summary>
        Task<Order?> GetByIdAsync(int orderId, bool includeItems = true);

        /// <summary>Returns all orders placed by a specific user.</summary>
        Task<IEnumerable<Order>> GetByUserIdAsync(int userId, bool includeItems = true);

        /// <summary>Creates a new order with its items.</summary>
        Task<Order> CreateOrderAsync(int userId, string? paymentMethod, string? deliveryAddress, IEnumerable<OrderItem> items);

        /// <summary>Creates an order directly from user's active cart and clears cart afterwards.</summary>
        Task<Order> CheckoutFromCartAsync(int userId, string? paymentMethod, string? deliveryAddress);

        /// <summary>Inserts an order entity.</summary>
        Task AddAsync(Order entity);

        /// <summary>Updates order details.</summary>
        Task UpdateAsync(Order entity);

        /// <summary>Updates order status (e.g., Pending, Processing, Shipped, Delivered, Cancelled).</summary>
        Task UpdateStatusAsync(int orderId, string newStatus);

        /// <summary>Deletes an order by primary key.</summary>
        Task DeleteAsync(int orderId);

        /// <summary>Deletes all orders.</summary>
        Task DeleteAllAsync();

        /// <summary>Returns a paginated slice of orders.</summary>
        Task<PaginationModel<Order>> GetPaginatedAsync(int pageNumber, int pageSize, int? userId = null, string? status = null);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.OrderModule
{
    public interface IOrderItemRepository
    {
        Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId);
        Task<OrderItem?> GetByIdAsync(int orderItemId);
        Task AddAsync(OrderItem entity);
        Task UpdateAsync(OrderItem entity);
        Task DeleteAsync(int orderItemId);
        Task DeleteByOrderIdAsync(int orderId);
    }
}

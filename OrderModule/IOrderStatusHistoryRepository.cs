using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.OrderModule
{
    public interface IOrderStatusHistoryRepository
    {
        Task<IEnumerable<OrderStatusHistory>> GetAllAsync();
        Task<OrderStatusHistory?> GetByIdAsync(int id);
        Task<IEnumerable<OrderStatusHistory>> GetByOrderIdAsync(int orderId);
        Task AddAsync(OrderStatusHistory entity);
        Task UpdateAsync(OrderStatusHistory entity);
        Task DeleteAsync(int id);
    }
}

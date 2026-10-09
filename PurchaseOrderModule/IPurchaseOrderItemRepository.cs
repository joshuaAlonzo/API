using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.PurchaseOrderModule
{
    public interface IPurchaseOrderItemRepository
    {
        Task<IEnumerable<PurchaseOrderItem>> GetAllAsync();
        Task<PurchaseOrderItem?> GetByIdAsync(int id);
        Task<IEnumerable<PurchaseOrderItem>> GetByPurchaseOrderIdAsync(int purchaseOrderId);
        Task AddAsync(PurchaseOrderItem entity);
        Task UpdateAsync(PurchaseOrderItem entity);
        Task DeleteAsync(int id);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.PurchaseOrderModule
{
    public interface IPurchaseOrderRepository
    {
        Task<IEnumerable<PurchaseOrder>> GetAllAsync();
        Task<PurchaseOrder?> GetByIdAsync(int purchaseOrderId);
        Task<PurchaseOrder?> GetByNumberAsync(string purchaseOrderNumber);
        Task AddAsync(PurchaseOrder entity);
        Task UpdateAsync(PurchaseOrder entity);
        Task DeleteAsync(int purchaseOrderId);
    }
}

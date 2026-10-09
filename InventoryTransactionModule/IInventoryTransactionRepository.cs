using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.InventoryTransactionModule
{
    public interface IInventoryTransactionRepository
    {
        Task<IEnumerable<InventoryTransaction>> GetAllAsync();
        Task<InventoryTransaction?> GetByIdAsync(int id);
        Task<IEnumerable<InventoryTransaction>> GetByMedicineIdAsync(int medicineId);
        Task AddAsync(InventoryTransaction entity);
        Task UpdateAsync(InventoryTransaction entity);
        Task DeleteAsync(int id);
    }
}

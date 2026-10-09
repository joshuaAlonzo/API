using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Main;

namespace Api.MedicineBatchModule
{
    public interface IMedicineBatchRepository
    {
        Task<IEnumerable<MedicineBatch>> GetAllAsync();
        Task<MedicineBatch?> GetByIdAsync(int batchId);
        Task<IEnumerable<MedicineBatch>> GetByMedicineIdAsync(int medicineId);
        Task AddAsync(MedicineBatch entity);
        Task UpdateAsync(MedicineBatch entity);
        Task DeleteAsync(int batchId);
    }
}

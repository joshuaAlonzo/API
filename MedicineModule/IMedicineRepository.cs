using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Main;

namespace Api.MedicineModule
{
    public interface IMedicineRepository
    {
        /// <summary>Returns all medicines.</summary>
        Task<IEnumerable<Medicine>> GetAllAsync();

        /// <summary>Returns a single medicine by primary key.</summary>
        Task<Medicine?> GetByIdAsync(int medicineId);

        /// <summary>Returns medicines associated with a category.</summary>
        Task<IEnumerable<Medicine>> GetByCategoryIdAsync(int categoryId);

        /// <summary>Inserts a new medicine.</summary>
        Task AddAsync(Medicine entity);

        /// <summary>Updates an existing medicine.</summary>
        Task UpdateAsync(Medicine entity);

        /// <summary>Deletes a medicine by primary key.</summary>
        Task DeleteAsync(int medicineId);

        /// <summary>Deletes all medicines.</summary>
        Task DeleteAllAsync();

        /// <summary>LIKE-based search across brand_name, generic_name, manufacturer, and description.</summary>
        Task<IEnumerable<Medicine>> SearchAsync(string query);

        /// <summary>Returns a paginated slice with optional category and status filtering.</summary>
        Task<PaginationModel<Medicine>> GetPaginatedAsync(int pageNumber, int pageSize, int? categoryId = null, string? status = null);
    }
}

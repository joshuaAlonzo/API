using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Main;

namespace Api.CategoriesModule
{
    public interface ICategoryRepository
    {
        /// <summary>Returns all categories.</summary>
        Task<IEnumerable<Category>> GetAllAsync();

        /// <summary>Returns a single category by primary key.</summary>
        Task<Category?> GetByIdAsync(int categoryId);

        /// <summary>Returns a category by its exact name (case-insensitive).</summary>
        Task<Category?> GetByNameAsync(string categoryName);

        /// <summary>Inserts a new category.</summary>
        Task AddAsync(Category entity);

        /// <summary>Updates an existing category's name.</summary>
        Task UpdateAsync(Category entity);

        /// <summary>Deletes a category by primary key.</summary>
        Task DeleteAsync(int categoryId);

        /// <summary>Deletes all categories.</summary>
        Task DeleteAllAsync();

        /// <summary>LIKE-based name search.</summary>
        Task<IEnumerable<Category>> SearchAsync(string query);

        /// <summary>Returns a paginated slice.</summary>
        Task<PaginationModel<Category>> GetPaginatedAsync(int pageNumber, int pageSize);
    }
}

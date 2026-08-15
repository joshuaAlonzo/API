using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Main;

namespace Api.CartModule
{
    public interface ICartRepository
    {
        /// <summary>Returns all cart rows.</summary>
        Task<IEnumerable<Cart>> GetAllAsync();

        /// <summary>Returns a single cart item by primary key.</summary>
        Task<Cart?> GetByIdAsync(int cartId);

        /// <summary>Returns all cart items belonging to a specific user.</summary>
        Task<IEnumerable<Cart>> GetByUserIdAsync(int userId);

        /// <summary>Returns a specific cart item for a user + medicine combination.</summary>
        Task<Cart?> GetByUserAndMedicineAsync(int userId, int medicineId);

        /// <summary>Inserts a new cart item.</summary>
        Task AddAsync(Cart entity);

        /// <summary>Updates an existing cart item (quantity / subtotal).</summary>
        Task UpdateAsync(Cart entity);

        /// <summary>Deletes a single cart item by primary key.</summary>
        Task DeleteAsync(int cartId);

        /// <summary>Removes a specific medicine from a user's cart.</summary>
        Task DeleteByUserAndMedicineAsync(int userId, int medicineId);

        /// <summary>Clears all items from a user's cart.</summary>
        Task ClearCartAsync(int userId);

        /// <summary>Deletes every row in the cart table.</summary>
        Task DeleteAllAsync();

        /// <summary>Exact-match filter. Supported keys: cart_id, user_id, medicine_id.</summary>
        Task<IEnumerable<Cart>> GetFilteredExactAsync(IReadOnlyDictionary<string, object?> filters);

        /// <summary>Returns a paginated slice of cart rows.</summary>
        Task<PaginationModel<Cart>> GetPaginatedAsync(int pageNumber, int pageSize);
    }
}

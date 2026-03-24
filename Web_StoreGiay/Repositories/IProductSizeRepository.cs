using Web_StoreGiay.Models;

namespace Web_StoreGiay.Repositories
{
    public interface IProductSizeRepository
    {
        Task<IEnumerable<ProductSize>> GetAllAsync();
        Task<ProductSize> GetByIdAsync(int id);
        Task<IEnumerable<ProductSize>> GetByProductIdAsync(int productId);
        Task AddAsync(ProductSize productSize);
        Task UpdateAsync(ProductSize productSize);
        Task DeleteAsync(int id);
    }
}

using Web_StoreGiay.Models;
using Microsoft.EntityFrameworkCore;

namespace Web_StoreGiay.Repositories
{
    public class EFProductSizeRepository : IProductSizeRepository
    {
        private readonly ApplicationDbContext _context;

        public EFProductSizeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductSize>> GetAllAsync()
        {
            return await _context.ProductSizes.Include(ps => ps.Product).Include(ps => ps.Size).ToListAsync();
        }

        public async Task<ProductSize> GetByIdAsync(int id)
        {
            return await _context.ProductSizes.Include(ps => ps.Product).Include(ps => ps.Size)
                .FirstOrDefaultAsync(ps => ps.Id == id);
        }

        public async Task<IEnumerable<ProductSize>> GetByProductIdAsync(int productId)
        {
            return await _context.ProductSizes.Include(ps => ps.Size)
                .Where(ps => ps.ProductId == productId).ToListAsync();
        }

        public async Task AddAsync(ProductSize productSize)
        {
            _context.ProductSizes.Add(productSize);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProductSize productSize)
        {
            _context.ProductSizes.Update(productSize);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var productSize = await _context.ProductSizes.FirstOrDefaultAsync(ps => ps.SizeId == id);
            if (productSize != null)
            {
                _context.ProductSizes.Remove(productSize);
                await _context.SaveChangesAsync();
            }
        }
    }
}

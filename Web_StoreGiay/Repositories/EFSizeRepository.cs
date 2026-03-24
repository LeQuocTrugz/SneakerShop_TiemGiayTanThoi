using Microsoft.EntityFrameworkCore;
using Web_StoreGiay.Models;

namespace Web_StoreGiay.Repositories
{
    public class EFSizeRepository : ISizeRepository
    {
        private readonly ApplicationDbContext _context;

        public EFSizeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Size>> GetAllAsync()
        {
            return await _context.Sizes.ToListAsync();
        }

        public async Task<Size> GetByIdAsync(int id)
        {
            return await _context.Sizes.FindAsync(id);
        }

        public async Task AddAsync(Size size)
        {
            _context.Sizes.Add(size);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Size size)
        {
            _context.Sizes.Update(size);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var size = await _context.Sizes.FindAsync(id);
            _context.Sizes.Remove(size);
            await _context.SaveChangesAsync();
        }
    }
}

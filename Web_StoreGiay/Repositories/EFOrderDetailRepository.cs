using Web_StoreGiay.Models;
using Microsoft.EntityFrameworkCore;
namespace Web_StoreGiay.Repositories
{
    public class EFOrderDetailRepository : IOrderDetailRepository
    {
        private readonly ApplicationDbContext _context;

        public EFOrderDetailRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderDetail>> GetAllAsync()
        {
            return await _context.OrderDetails.Include(od => od.Order).Include(od => od.Product).ToListAsync();
        }

        public async Task<OrderDetail> GetByIdAsync(int id)
        {
            return await _context.OrderDetails.Include(od => od.Order).Include(od => od.Product)
                .FirstOrDefaultAsync(od => od.Id == id);
        }

        public async Task AddAsync(OrderDetail orderDetail)
        {
            _context.OrderDetails.Add(orderDetail);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(OrderDetail orderDetail)
        {
            _context.OrderDetails.Update(orderDetail);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var orderDetail = await _context.OrderDetails.FindAsync(id);
            _context.OrderDetails.Remove(orderDetail);
            await _context.SaveChangesAsync();
        }
    }
}

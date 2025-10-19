using Microsoft.EntityFrameworkCore;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.Models;

namespace Mo_DataAccess.Services
{
    public class OrderProductServices : IOrderProductServices
    {
        private readonly SwpGroup6Context _context;

        public OrderProductServices(SwpGroup6Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderProduct>> GetAllAsync()
        {
            return await _context.OrderProducts
                .Include(o => o.Account)
                .Include(o => o.ProductVariant)
                .ToListAsync();
        }

        public async Task<OrderProduct?> GetByIdAsync(long id)
        {
            return await _context.OrderProducts
                .Include(o => o.Account)
                .Include(o => o.ProductVariant)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<OrderProduct> CreateAsync(OrderProduct order)
        {
            _context.OrderProducts.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<OrderProduct?> UpdateAsync(long id, OrderProduct order)
        {
            var existing = await _context.OrderProducts.FindAsync(id);
            if (existing == null)
                return null;

            existing.AccountId = order.AccountId;
            existing.ProductVariantId = order.ProductVariantId;
            existing.TotalAmount = order.TotalAmount;
            existing.Quantity = order.Quantity;
            existing.Status = order.Status;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var existing = await _context.OrderProducts.FindAsync(id);
            if (existing == null)
                return false;

            _context.OrderProducts.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

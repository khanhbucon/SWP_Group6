using Microsoft.EntityFrameworkCore;
using Mo_DataAccess.Repo;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mo_DataAccess.Services

{
    public class OrderProductServices : GenericRepository<OrderProduct>, IOrderProductServices
    {
        private readonly SwpGroup6Context
 _context;

        public OrderProductServices(SwpGroup6Context
 context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderProduct>> GetOrdersByAccountIdAsync(long accountId)
        {
            return await _context.OrderProducts
                .Include(o => o.ProductVariant)
                .Include(o => o.Account)
                .Where(o => o.AccountId == accountId)
                .ToListAsync();
        }

        public async Task UpdateStatusAsync(long orderId, string status)
        {
            var order = await _context.OrderProducts.FindAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                _context.OrderProducts.Update(order);
                await _context.SaveChangesAsync();
            }
        }
    }
}

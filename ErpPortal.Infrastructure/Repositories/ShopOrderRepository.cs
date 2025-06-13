using ErpPortal.Application.Interfaces.Repositories;
using ErpPortal.Domain.Entities;
using ErpPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ErpPortal.Infrastructure.Repositories
{
    public class ShopOrderRepository : IShopOrderRepository
    {
        private readonly ErpPortalDbContext _context;

        public ShopOrderRepository(ErpPortalDbContext context)
        {
            _context = context;
        }

        public async Task<ShopOrder?> GetByOrderNoAsync(string orderNo)
        {
            return await _context.ShopOrders.FindAsync(orderNo);
        }

        public async Task UpsertRangeAsync(IEnumerable<ShopOrder> orders)
        {
            foreach (var order in orders)
            {
                var existing = await _context.ShopOrders.AsNoTracking().FirstOrDefaultAsync(o => o.OrderNo == order.OrderNo);
                if (existing == null)
                {
                    _context.ShopOrders.Add(order);
                }
                else
                {
                    // Keep original PK, update scalar fields
                    order.Operations = existing.Operations; // avoid overwriting navigation accidentally
                    _context.Entry(existing).CurrentValues.SetValues(order);
                    _context.Entry(existing).State = EntityState.Modified;
                }
            }
            await _context.SaveChangesAsync();
        }
    }
} 
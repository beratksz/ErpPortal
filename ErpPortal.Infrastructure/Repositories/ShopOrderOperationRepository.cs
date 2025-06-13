using ErpPortal.Application.Interfaces.Repositories;
using ErpPortal.Domain.Entities;
using ErpPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // ILogger için
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ErpPortal.Infrastructure.Repositories
{
    public class ShopOrderOperationRepository : IShopOrderOperationRepository
    {
        private readonly ErpPortalDbContext _context;
        private readonly ILogger<ShopOrderOperationRepository> _logger;

        public ShopOrderOperationRepository(ErpPortalDbContext context, ILogger<ShopOrderOperationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ShopOrderOperation>> GetAllAsync()
        {
            return await _context.ShopOrderOperations.ToListAsync();
        }

        public async Task<ShopOrderOperation?> GetByIdAsync(int id)
        {
            return await _context.ShopOrderOperations.FindAsync(id);
        }

        public async Task<ShopOrderOperation?> GetByOrderAndOperationNoAsync(string orderNo, int operationNo)
        {
            return await _context.ShopOrderOperations
                .FirstOrDefaultAsync(op => op.OrderNo == orderNo && op.OperationNo == operationNo);
        }

        public async Task<IEnumerable<ShopOrderOperation>> GetByWorkCenterAsync(string workCenterCode)
        {
            return await _context.ShopOrderOperations
                .Where(op => op.WorkCenterCode == workCenterCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<ShopOrderOperation>> GetByOrderNoAsync(string orderNo)
        {
            return await _context.ShopOrderOperations
                .Where(o => o.OrderNo == orderNo)
                .ToListAsync();
        }

        public async Task<IEnumerable<ShopOrderOperation>> GetPendingSyncOperationsAsync()
        {
            return await _context.ShopOrderOperations
                .Where(o => o.IsSyncPending)
                .ToListAsync();
        }

        public async Task<IEnumerable<ShopOrderOperation>> GetAwaitingQualityAsync()
        {
            return await _context.ShopOrderOperations
                .Where(o => o.IsAwaitingQuality)
                .ToListAsync();
        }

        public async Task AddAsync(ShopOrderOperation operation)
        {
            // Eğer ilgili ShopOrder kaydı veritabanında yoksa önce temel bir kayıt oluştur.
            var existingOrder = await _context.ShopOrders.FindAsync(operation.OrderNo);
            if (existingOrder == null)
            {
                var stubOrder = new ShopOrder
                {
                    OrderNo = operation.OrderNo,
                    Description = operation.OperationDescription,
                    PartNo = operation.PartNo,
                    PartDescription = operation.PartDescription,
                    Status = operation.Status,
                    Quantity = operation.RevisedQtyDue
                };
                await _context.ShopOrders.AddAsync(stubOrder);
            }

            await _context.ShopOrderOperations.AddAsync(operation);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ShopOrderOperation operation)
        {
            _context.Entry(operation).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task AddOrUpdateRangeAsync(IEnumerable<ShopOrderOperation> operations)
        {
            foreach (var operation in operations)
            {
                var existingOperation = await _context.ShopOrderOperations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(op => op.OrderNo == operation.OrderNo && op.OperationNo == operation.OperationNo);

                // Sipariş kaydı yoksa ekle
                if (!await _context.ShopOrders.AnyAsync(o => o.OrderNo == operation.OrderNo))
                {
                    var stubOrder = new ShopOrder
                    {
                        OrderNo = operation.OrderNo,
                        Description = operation.OperationDescription,
                        PartNo = operation.PartNo,
                        PartDescription = operation.PartDescription,
                        Status = operation.Status,
                        Quantity = operation.RevisedQtyDue
                    };
                    _context.ShopOrders.Add(stubOrder);
                }

                if (existingOperation == null)
                {
                    _context.ShopOrderOperations.Add(operation);
                }
                else
                {
                    operation.Id = existingOperation.Id; // ID'yi koru
                    _context.ShopOrderOperations.Update(operation);
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var operation = await _context.ShopOrderOperations.FindAsync(id);
            if (operation != null)
            {
                _context.ShopOrderOperations.Remove(operation);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SyncOperationsAsync(IEnumerable<ShopOrderOperation> operations)
        {
            foreach (var operation in operations)
            {
                var existing = await _context.ShopOrderOperations
                    .FirstOrDefaultAsync(o => o.OrderNo == operation.OrderNo && o.OperationNo == operation.OperationNo);

                if (existing != null)
                {
                    _context.Entry(existing).CurrentValues.SetValues(operation);
                }
                else
                {
                    _context.ShopOrderOperations.Add(operation);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}

using ErpPortal.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpPortal.Application.Interfaces.Repositories
{
    public interface IShopOrderOperationRepository
    {
        Task<ShopOrderOperation?> GetByIdAsync(int id);
        Task<IEnumerable<ShopOrderOperation>> GetAllAsync();
        Task<ShopOrderOperation?> GetByOrderAndOperationNoAsync(string orderNo, int operationNo);
        Task<IEnumerable<ShopOrderOperation>> GetByWorkCenterAsync(string workCenterCode);
        Task<IEnumerable<ShopOrderOperation>> GetPendingSyncOperationsAsync();
        Task<IEnumerable<ShopOrderOperation>> GetAwaitingQualityAsync();
        Task AddAsync(ShopOrderOperation operation);
        Task UpdateAsync(ShopOrderOperation operation);
        Task AddOrUpdateRangeAsync(IEnumerable<ShopOrderOperation> operations);
        Task DeleteAsync(int id);
        Task SyncOperationsAsync(IEnumerable<ShopOrderOperation> operations);
        Task<int> SaveChangesAsync();
    }
}

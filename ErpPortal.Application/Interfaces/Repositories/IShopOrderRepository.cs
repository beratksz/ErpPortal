namespace ErpPortal.Application.Interfaces.Repositories
{
    using ErpPortal.Domain.Entities;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IShopOrderRepository
    {
        Task<ShopOrder?> GetByOrderNoAsync(string orderNo);
        Task UpsertRangeAsync(IEnumerable<ShopOrder> orders);
    }
} 
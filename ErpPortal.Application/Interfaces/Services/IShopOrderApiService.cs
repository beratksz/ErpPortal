using ErpPortal.Application.Models.ShopOrder;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpPortal.Application.Interfaces.Services
{
    public interface IShopOrderApiService
    {
        Task<IEnumerable<ShopOrderDto>> GetShopOrdersAsync(string workCenterCode);
        Task<ShopOrderDetailsDto> GetShopOrderDetailsAsync(string orderNo);
        Task<ShopOrderOperationDto> UpdateOperationStatusAsync(string orderNo, string operationNo, OperationStatusUpdateDto updateDto);
        Task<bool> PatchOperationStatusAsync(string orderNo, int operationNo, string status, string? reason, string releaseNo = "*", string sequenceNo = "*", string? etag = null);
        Task<bool> PatchOperationDetailsAsync(string orderNo, int operationNo, IDictionary<string, object> patchData, string? etag = null);
        Task<IEnumerable<ErpPortal.Domain.Entities.ShopOrderOperation>> GetOperationsAsync(string workCenterCode);
        Task<bool> ResumeOperationAsync(string orderNo, int operationNo, string releaseNo = "*", string sequenceNo = "*");
    }
} 
using ErpPortal.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpPortal.Application.Interfaces.Services
{
    public interface IShopOrderService
    {
        Task<IEnumerable<ShopOrderOperation>> GetOperationsForWorkCenterAsync(string workCenterNo);
        // İş merkezine ait operasyonları yerel veritabanından getirir.
        Task<ShopOrderOperation?> GetOperationDetailsAsync(string orderNo, int operationNo);

        // Operasyon bilgilerini günceller.
        // Hem yerel DB'yi günceller hem de API'ye bildirir.
        Task<bool> UpdateOperationAsync(ShopOrderOperation operation);

        // Bir operasyonu başlatır.
        // Hem yerel DB'yi günceller hem de API'ye bildirir.
        Task<bool> StartOperationAsync(string orderNo, int operationNo, string userName);

        // Bir operasyonu durdurur.
        // Hem yerel DB'yi günceller hem de API'ye bildirir.
        Task<bool> StopOperationAsync(string orderNo, int operationNo, string userName, string? reason, decimal quantityCompleted, decimal quantityScrapped);

        // Bir operasyonu devam ettirir (durdurulduktan sonra).
        Task<bool> ResumeOperationAsync(string orderNo, int operationNo, string userName);

        // Bir operasyonu tamamlar, onay ve hurda miktarını bildirir.
        // Hem yerel DB'yi günceller hem de API'ye bildirir.
        Task<bool> ReportCompletionAsync(string orderNo, int operationNo, string userName, decimal quantityCompleted, decimal quantityScrapped);

        Task<ShopOrderOperation> GetOperationAsync(int id);
        Task<IEnumerable<ShopOrderOperation>> GetOperationsByWorkCenterAsync(string workCenterCode);
        Task<bool> StartOperationAsync(int id, string reportedBy);
        Task<bool> CompleteOperationAsync(int id, decimal qtyComplete, decimal qtyScrapped, string reportedBy);
        Task<bool> InterruptOperationAsync(int id, string reason, string reportedBy);
        Task<bool> ResumeOperationAsync(int id, string reportedBy);

        // Parça parça tamamlanan veya hurdaya ayrılan miktarları günceller
        Task<bool> UpdateQuantitiesAsync(string orderNo, int operationNo, string userName, decimal addedCompleted, decimal addedScrapped, string? reason);
    }
} 
namespace ErpPortal.Application.Interfaces.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ErpPortal.Domain.Entities;

    public interface IQualityService
    {
        /// <summary>
        /// Kalite onayı bekleyen operasyonları getirir.
        /// </summary>
        Task<IEnumerable<ShopOrderOperation>> GetAwaitingQualityOperationsAsync();

        /// <summary>
        /// Hurda içeren ve NCR açık olan operasyonu kalite tarafından onaylar.
        /// NCR kapatılır, operasyon bekleme flag'i kaldırılır.
        /// </summary>
        Task<bool> ApproveAsync(string orderNo, int operationNo, string approvedBy, string disposition, string? notes);
    }
} 
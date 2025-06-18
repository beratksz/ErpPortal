namespace ErpPortal.Application.Interfaces.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ErpPortal.Application.Models.Quality;

    /// <summary>
    /// IFS Non-Conformance Report (NCR) servis çağrıları.
    /// NCR sorgulama ve kapatma işlemleri için kullanılır.
    /// </summary>
    public interface INonConformanceApiService
    {
        /// <summary>
        /// Belirli iş emri / operasyon için kapatılmamış NCR kayıtlarını getirir.
        /// </summary>
        Task<IEnumerable<NcrDto>> GetOpenNcrsAsync(string orderNo, int operationNo);

        /// <summary>
        /// NCR kaydını kapatır (Disposition vererek).
        /// </summary>
        Task<bool> CloseNcrAsync(string ncrNo, string disposition, string? notes, string? closedBy = null);

        /// <summary>
        /// Hurda raporuna bağlı yeni bir NCR kaydı oluşturur ve oluşturulan NcrNo'yu döner.
        /// </summary>
        Task<string?> CreateNcrAsync(string orderNo, int operationNo, decimal scrapQty, string description);
    }
} 
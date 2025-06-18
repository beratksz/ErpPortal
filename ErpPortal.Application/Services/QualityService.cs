using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErpPortal.Application.Interfaces.Repositories;
using ErpPortal.Application.Interfaces.Services;
using ErpPortal.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ErpPortal.Application.Services
{
    public class QualityService : IQualityService
    {
        private readonly IShopOrderOperationRepository _operationRepository;
        private readonly INonConformanceApiService _ncrApi;
        private readonly ILogger<QualityService> _logger;

        public QualityService(IShopOrderOperationRepository operationRepository,
                               INonConformanceApiService ncrApi,
                               ILogger<QualityService> logger)
        {
            _operationRepository = operationRepository;
            _ncrApi = ncrApi;
            _logger = logger;
        }

        public async Task<IEnumerable<ShopOrderOperation>> GetAwaitingQualityOperationsAsync()
        {
            return await _operationRepository.GetAwaitingQualityAsync();
        }

        public async Task<bool> ApproveAsync(string orderNo, int operationNo, string approvedBy, string disposition, string? notes)
        {
            try
            {
                var ncrs = await _ncrApi.GetOpenNcrsAsync(orderNo, operationNo);
                var ncr = ncrs.FirstOrDefault();
                if (ncr == null)
                {
                    _logger.LogWarning("ApproveAsync: NCR bulunamadı {OrderNo}/{OpNo}", orderNo, operationNo);
                    return false;
                }

                var ok = await _ncrApi.CloseNcrAsync(ncr.NcrNo, disposition, notes, approvedBy);
                if (!ok) return false;

                var op = await _operationRepository.GetByOrderAndOperationNoAsync(orderNo, operationNo);
                if (op == null)
                {
                    _logger.LogWarning("ApproveAsync: Operation DB'de yok {OrderNo}/{OpNo}", orderNo, operationNo);
                    return false;
                }

                op.IsAwaitingQuality = false;
                op.LastSyncError = null;
                op.IsSyncPending = true; // AutoCloseService kapatacak
                // Disposition sonucu saklamak isterseniz custom alan ekleyin.

                await _operationRepository.UpdateAsync(op);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ApproveAsync hata {OrderNo}/{OpNo}", orderNo, operationNo);
                return false;
            }
        }
    }
} 
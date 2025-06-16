using ErpPortal.Application.Interfaces.Repositories;
using ErpPortal.Application.Interfaces.Services;
using ErpPortal.Application.Models.ShopOrder;
using ErpPortal.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ErpPortal.Application.Services
{
    public class ShopOrderService : IShopOrderService
    {
        private readonly IShopOrderOperationRepository _operationRepository;
        private readonly IWorkLogRepository _workLogRepository;
        private readonly IUserService _userService;
        private readonly IShopOrderApiService _apiService;
        private readonly IShopOrderRepository _orderRepository;
        private readonly ILogger<ShopOrderService> _logger;

        public ShopOrderService(
            IShopOrderOperationRepository operationRepository,
            IWorkLogRepository workLogRepository,
            IUserService userService,
            IShopOrderApiService apiService,
            IShopOrderRepository orderRepository,
            ILogger<ShopOrderService> logger)
        {
            _operationRepository = operationRepository;
            _workLogRepository = workLogRepository;
            _userService = userService;
            _apiService = apiService;
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<ShopOrderOperation>> GetOperationsForWorkCenterAsync(string workCenterNo)
        {
            try
            {
                // API'den en güncel verileri çek
                var apiOperations = await _apiService.GetOperationsAsync(workCenterNo);

                // Önce ShopOrders tablosunu güncelle
                var shopOrders = apiOperations
                    .GroupBy(o => o.OrderNo)
                    .Select(g => new ShopOrder
                    {
                        OrderNo = g.Key,
                        Description = g.First().OperationDescription,
                        PartNo = g.First().PartNo,
                        PartDescription = g.First().PartDescription,
                        Quantity = g.First().RevisedQtyDue,
                        Status = g.First().Status
                    });

                await _orderRepository.UpsertRangeAsync(shopOrders);

                // Veritabanındaki mevcut operasyonları al
                var dbOperations = await _operationRepository.GetByWorkCenterAsync(workCenterNo);
                var dbOperationsDict = dbOperations.ToDictionary(o => o.OrderNo + "_" + o.OperationNo);

                foreach (var apiOp in apiOperations)
                {
                    var key = apiOp.OrderNo + "_" + apiOp.OperationNo;
                    if (dbOperationsDict.TryGetValue(key, out var dbOp))
                    {
                        // Eğer yerel kayıt hala senkronizasyon bekliyorsa, API'den gelen verilerle üzerine yazma
                        if (dbOp.IsSyncPending)
                        {
                            continue;
                        }
                        // Var olanı güncelle
                        dbOp.Status = apiOp.Status;
                        dbOp.OperStatusCode = apiOp.OperStatusCode;
                        dbOp.RevisedQtyDue = apiOp.RevisedQtyDue;
                        dbOp.ReleaseNo = apiOp.ReleaseNo;
                        dbOp.SequenceNo = apiOp.SequenceNo;
                        dbOp.WorkCenterNo = apiOp.WorkCenterNo;
                        await _operationRepository.UpdateAsync(dbOp);
                    }
                    else
                    {
                        // Yeni operasyonu ekle
                        await _operationRepository.AddAsync(apiOp);
                    }
                }
                
                return await _operationRepository.GetByWorkCenterAsync(workCenterNo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Orchestration error in GetOperationsForWorkCenterAsync for {WorkCenterNo}", workCenterNo);
                // Dış API kaynaklı hatalarda uygulamanın çalışmaya devam edebilmesi için
                // sadece veritabanındaki mevcut kayıtları dön ve hata fırlatma.
                return await _operationRepository.GetByWorkCenterAsync(workCenterNo);
            }
        }

        public async Task<bool> StartOperationAsync(string orderNo, int operationNo, string userName)
        {
            try
            {
                var operation = await _operationRepository.GetByOrderAndOperationNoAsync(orderNo, operationNo);
                if (operation == null)
                {
                    _logger.LogWarning("StartOperationAsync: Operation not found in DB for {OrderNo}/{OperationNo}", orderNo, operationNo);
                    return false;
                }

                operation.Status = "STARTED";
                operation.OperStatusCode = "InProcess";
                operation.StartTime = DateTime.UtcNow;
                operation.ActualStartDate = operation.StartTime;
                operation.EndTime = null;
                operation.AssignedTo = userName;

                await _operationRepository.UpdateAsync(operation);
                
                // API'ye durumu bildir
                var success = await _apiService.PatchOperationStatusAsync(orderNo, operationNo, "STARTED", null, operation.ReleaseNo, operation.SequenceNo, operation.GetEtag());
                if (!success)
                {
                    _logger.LogError("Failed to patch operation status to STARTED on API for {OrderNo}/{OperationNo}", orderNo, operationNo);
                    // Burada bir telafi mekanizması düşünülebilir. Şimdilik sadece logluyoruz.
                }
                
                // WorkLog: yeni kayıt
                var user = await _userService.GetUserByUsernameAsync(userName);
                if (user != null)
                {
                    var log = new WorkLog
                    {
                        OrderNo = orderNo,
                        OperationNo = operationNo,
                        UserId = user.Id,
                        StartTime = operation.StartTime!.Value,
                        Status = "ACTIVE"
                    };
                    await _workLogRepository.AddAsync(log);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in StartOperationAsync for {OrderNo}/{OperationNo}", orderNo, operationNo);
                return false;
            }
        }

        public async Task<bool> StopOperationAsync(string orderNo, int operationNo, string userName, string? reason, decimal quantityCompleted, decimal quantityScrapped)
        {
            try
            {
                var operation = await _operationRepository.GetByOrderAndOperationNoAsync(orderNo, operationNo);
                if (operation == null)
                {
                    _logger.LogWarning("StopOperationAsync: Operation not found in DB for {OrderNo}/{OperationNo}", orderNo, operationNo);
                    return false;
                }

                operation.Status = "STOPPED";
                operation.OperStatusCode = "Interruption";
                operation.EndTime = DateTime.UtcNow;
                operation.InterruptionReason = reason;
                // Eğer tamamlanan/hurda miktarları 0 gönderildiyse mevcut değerleri KORU.
                if (quantityCompleted > 0 || quantityScrapped > 0)
                {
                    operation.QuantityCompleted = quantityCompleted;
                    operation.QuantityScrapped = quantityScrapped;
                    operation.QtyComplete = quantityCompleted;
                    operation.QtyScrapped = quantityScrapped;
                }
                operation.ReportedBy = userName;
                operation.LastInterruptionTime = operation.EndTime;

                await _operationRepository.UpdateAsync(operation);

                var user = await _userService.GetUserByUsernameAsync(userName);
                if (user != null)
                {
                    var log = await _workLogRepository.GetLatestLogAsync(orderNo, operationNo, user.Id);
                    if (log != null && log.EndTime == null)
                    {
                        log.EndTime = operation.EndTime;
                        log.Duration = log.EndTime - log.StartTime;
                        log.Status = "PAUSED";
                        log.Description = reason;
                        await _workLogRepository.UpdateAsync(log);

                        operation.TotalInterruptionDuration += log.Duration ?? TimeSpan.Zero;
                    }
                }

                // IFS'e NoteText olarak sadece kullanıcı adını gönder, durma sebebi sadece local DB'de kalsın
                await _apiService.PatchOperationStatusAsync(orderNo, operationNo, "STOPPED", null, operation.ReleaseNo, operation.SequenceNo, operation.GetEtag());

                // Ardından miktar bilgilerini gönder
                var patchData = new Dictionary<string, object>
                {
                    { "OperStatusCode", "Interruption" },
                    { "QtyComplete", operation.QuantityCompleted }, // toplam değerler
                    { "QtyScrapped", operation.QuantityScrapped },
                    { "NoteText", string.IsNullOrEmpty(reason) ? userName : reason },
                    { "ReleaseNo", operation.ReleaseNo },
                    { "SequenceNo", operation.SequenceNo }
                };

                var success = await _apiService.PatchOperationDetailsAsync(orderNo, operationNo, patchData, operation.GetEtag());
                if (!success)
                {
                    _logger.LogError("Failed to patch operation details on API for {OrderNo}/{OperationNo}", orderNo, operationNo);
                    // İşlem API'ye senkronize edilemedi, yerel kayıt güncellenir
                    operation.IsSyncPending = true;
                    operation.LastSyncError = $"STOP PATCH failed at {DateTime.UtcNow:O}";
                }
                else
                {
                    operation.IsSyncPending = false;
                    operation.LastSyncError = null;
                }

                await _operationRepository.UpdateAsync(operation);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in StopOperationAsync for {OrderNo}/{OperationNo}", orderNo, operationNo);
                return false;
            }
        }

        public async Task<ShopOrderOperation?> GetOperationDetailsAsync(string orderNo, int operationNo)
        {
            return await _operationRepository.GetByOrderAndOperationNoAsync(orderNo, operationNo);
        }

        public async Task<bool> UpdateOperationAsync(ShopOrderOperation operation)
        {
            try
            {
                await _operationRepository.UpdateAsync(operation);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateOperationAsync failed for {OrderNo}/{OperationNo}", operation.OrderNo, operation.OperationNo);
                return false;
            }
        }

        public async Task<bool> ResumeOperationAsync(string orderNo, int operationNo, string userName)
        {
            try
            {
                var operation = await _operationRepository.GetByOrderAndOperationNoAsync(orderNo, operationNo);
                if (operation == null) return false;

                if (operation.Status != "STOPPED") return false;

                operation.Status = "STARTED";
                operation.OperStatusCode = "InProcess";
                operation.InterruptionReason = null;
                operation.StartTime = DateTime.UtcNow;
                operation.ActualStartDate = operation.StartTime;
                operation.AssignedTo = userName;

                await _operationRepository.UpdateAsync(operation);

                var success = await _apiService.PatchOperationStatusAsync(orderNo, operationNo, "STARTED", null, operation.ReleaseNo, operation.SequenceNo, operation.GetEtag());
                if (!success)
                {
                    _logger.LogWarning("API patch resume failed for {OrderNo}/{OperationNo}", orderNo, operationNo);
                }

                // WorkLog: yeni kayıt
                var user = await _userService.GetUserByUsernameAsync(userName);
                if (user != null)
                {
                    var log = new WorkLog
                    {
                        OrderNo = orderNo,
                        OperationNo = operationNo,
                        UserId = user.Id,
                        StartTime = DateTime.UtcNow,
                        Status = "ACTIVE"
                    };
                    await _workLogRepository.AddAsync(log);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ResumeOperationAsync failed for {OrderNo}/{OperationNo}", orderNo, operationNo);
                return false;
            }
        }

        public async Task<bool> ReportCompletionAsync(string orderNo, int operationNo, string userName, decimal quantityCompleted, decimal quantityScrapped)
        {
            try
            {
                var operation = await _operationRepository.GetByOrderAndOperationNoAsync(orderNo, operationNo);
                if (operation == null) return false;

                // Kural: miktarlar toplamı due qty olmalı
                var total = quantityCompleted + quantityScrapped;
                if (total != operation.RevisedQtyDue)
                {
                    _logger.LogWarning("Qty mismatch on completion for {OrderNo}/{OperationNo}. Completed+Scrap={Total} != Due {Due}", orderNo, operationNo, total, operation.RevisedQtyDue);
                    return false;
                }

                operation.Status = "COMPLETED";
                operation.OperStatusCode = "Closed";
                operation.EndTime = DateTime.UtcNow;
                operation.OpFinishDate = operation.EndTime.Value;
                operation.QuantityCompleted = quantityCompleted;
                operation.QuantityScrapped = quantityScrapped;
                operation.QtyComplete = quantityCompleted;
                operation.QtyScrapped = quantityScrapped;
                operation.ReportedBy = userName;

                await _operationRepository.UpdateAsync(operation);

                // IFS'te önce statü kapatılır
                await _apiService.PatchOperationStatusAsync(orderNo, operationNo, "COMPLETED", null, operation.ReleaseNo, operation.SequenceNo, operation.GetEtag());

                var patchData = new Dictionary<string, object>
                {
                    { "OperStatusCode", "Closed" },
                    { "OpFinishDate", DateTime.UtcNow },
                    { "NoteText", userName },
                    { "ReleaseNo", operation.ReleaseNo },
                    { "SequenceNo", operation.SequenceNo }
                };

                var success = await _apiService.PatchOperationDetailsAsync(orderNo, operationNo, patchData, operation.GetEtag());
                if (!success)
                {
                    _logger.LogWarning("API patch complete failed for {OrderNo}/{OperationNo}", orderNo, operationNo);
                    operation.IsSyncPending = true;
                    operation.LastSyncError = $"COMPLETE PATCH failed at {DateTime.UtcNow:O}";
                }
                else
                {
                    operation.IsSyncPending = false;
                    operation.LastSyncError = null;
                }

                await _operationRepository.UpdateAsync(operation);
                
                // WorkLog: son kayıt
                var user = await _userService.GetUserByUsernameAsync(userName);
                if (user != null)
                {
                    var log = await _workLogRepository.GetLatestLogAsync(orderNo, operationNo, user.Id);
                    if (log != null && log.EndTime == null)
                    {
                        log.EndTime = DateTime.UtcNow;
                        log.Duration = log.EndTime - log.StartTime;
                        log.Status = "COMPLETED";
                        await _workLogRepository.UpdateAsync(log);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ReportCompletionAsync failed for {OrderNo}/{OperationNo}", orderNo, operationNo);
                return false;
            }
        }

        // ID-based helper methods ------------------------------------------------
        public async Task<ShopOrderOperation> GetOperationAsync(int id)
        {
            var op = await _operationRepository.GetByIdAsync(id);
            if (op == null)
            {
                throw new KeyNotFoundException($"Operation with id {id} not found");
            }
            return op;
        }

        public async Task<IEnumerable<ShopOrderOperation>> GetOperationsByWorkCenterAsync(string workCenterCode)
        {
            return await _operationRepository.GetByWorkCenterAsync(workCenterCode);
        }

        public async Task<bool> StartOperationAsync(int id, string reportedBy)
        {
            var op = await GetOperationAsync(id);
            return await StartOperationAsync(op.OrderNo, op.OperationNo, reportedBy);
        }

        public async Task<bool> CompleteOperationAsync(int id, decimal qtyComplete, decimal qtyScrapped, string reportedBy)
        {
            var op = await GetOperationAsync(id);
            return await ReportCompletionAsync(op.OrderNo, op.OperationNo, reportedBy, qtyComplete, qtyScrapped);
        }

        public async Task<bool> InterruptOperationAsync(int id, string reason, string reportedBy)
        {
            var op = await GetOperationAsync(id);
            return await StopOperationAsync(op.OrderNo, op.OperationNo, reportedBy, reason, op.QuantityCompleted, op.QuantityScrapped);
        }

        public async Task<bool> ResumeOperationAsync(int id, string reportedBy)
        {
            var op = await GetOperationAsync(id);
            return await ResumeOperationAsync(op.OrderNo, op.OperationNo, reportedBy);
        }

        public async Task<bool> PatchOperationDetailsAsync(string orderNo, int operationNo, IDictionary<string, object> patchData)
        {
            try
            {
                return await _apiService.PatchOperationDetailsAsync(orderNo, operationNo, patchData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API patch complete failed for {OrderNo}/{OperationNo}", orderNo, operationNo);
                return false;
            }
        }

        public async Task<bool> ResumeOperationAsync(string orderNo, int operationNo)
        {
            try
            {
                return await _apiService.ResumeOperationAsync(orderNo, operationNo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API resume operation failed for {OrderNo}/{OperationNo}", orderNo, operationNo);
                return false;
            }
        }

        // Yeni: Tamamlanan/Hurda miktarlarını parça parça güncelle
        public async Task<bool> UpdateQuantitiesAsync(string orderNo, int operationNo, string userName, decimal addedCompleted, decimal addedScrapped, string? reason)
        {
            try
            {
                var operation = await _operationRepository.GetByOrderAndOperationNoAsync(orderNo, operationNo);
                if (operation == null) return false;

                operation.QuantityCompleted += addedCompleted;
                operation.QuantityScrapped += addedScrapped;
                operation.QtyComplete = operation.QuantityCompleted;
                operation.QtyScrapped = operation.QuantityScrapped;

                // Hurda bildirildiyse WorkLog oluştur
                if(addedScrapped > 0)
                {
                    var user = await _userService.GetUserByUsernameAsync(userName);
                    if(user != null)
                    {
                        var log = new Domain.Entities.WorkLog
                        {
                            OrderNo = orderNo,
                            OperationNo = operationNo,
                            UserId = user.Id,
                            StartTime = DateTime.UtcNow,
                            EndTime = DateTime.UtcNow,
                            Duration = TimeSpan.Zero,
                            Status = "SCRAP",
                            Description = reason
                        };
                        await _workLogRepository.AddAsync(log);
                    }

                    // Hurda bildirildi, kalite kontrolü bekle
                    operation.IsAwaitingQuality = true;
                }

                var totalDone = operation.QuantityCompleted + operation.QuantityScrapped;
                var isCompleted = totalDone >= operation.RevisedQtyDue;

                // IFS tarafında QtyComplete / QtyScrapped alanları mutlak (toplam) değerler olarak
                // güncelleniyor. Bu nedenle her PATCH isteğinde operasyonun güncel TOPLAM
                // miktarlarını göndermek gerekiyor.
                var payload = new Dictionary<string, object>
                {
                    { "QtyComplete", operation.QuantityCompleted },   // toplam tamamlanan
                    { "QtyScrapped", operation.QuantityScrapped },     // toplam hurda
                    { "NoteText", string.IsNullOrEmpty(reason) ? userName : reason },
                    { "ReleaseNo", operation.ReleaseNo },
                    { "SequenceNo", operation.SequenceNo }
                };

                if (isCompleted)
                {
                    // Artık iş tamamen bitti → Closed statüsüyle tek bir PATCH
                    operation.Status = "COMPLETED";
                    operation.OperStatusCode = "Closed";
                    operation.EndTime = DateTime.UtcNow;
                    operation.OpFinishDate = operation.EndTime.Value;

                    payload["OperStatusCode"] = "Closed";
                    payload["OpFinishDate"] = operation.OpFinishDate;
                }
                else
                {
                    // Hâlâ devam ediyor → PartiallyReported statüsü
                    operation.OperStatusCode = "PartiallyReported";
                    payload["OperStatusCode"] = "PartiallyReported";
                }

                await _operationRepository.UpdateAsync(operation);

                var success = await _apiService.PatchOperationDetailsAsync(orderNo, operationNo, payload, operation.GetEtag());

                if (!success)
                {
                    operation.IsSyncPending = true;
                    operation.LastSyncError = $"{(isCompleted ? "COMPLETE" : "PARTIAL")} PATCH failed at {DateTime.UtcNow:O}";
                }
                else
                {
                    operation.IsSyncPending = false;
                    operation.LastSyncError = null;

                    if (isCompleted)
                    {
                        operation.IsAwaitingQuality = false;
                    }
                }

                await _operationRepository.UpdateAsync(operation);

                // WorkLog güncelle → Operasyon bittiyse aktif kaydı kapat
                if (isCompleted)
                {
                    var user = await _userService.GetUserByUsernameAsync(userName);
                    if (user != null)
                    {
                        var log = await _workLogRepository.GetLatestLogAsync(orderNo, operationNo, user.Id);
                        if (log != null && log.EndTime == null)
                        {
                            log.EndTime = DateTime.UtcNow;
                            log.Duration = log.EndTime - log.StartTime;
                            log.Status = "COMPLETED";
                            await _workLogRepository.UpdateAsync(log);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateQuantitiesAsync failed for {OrderNo}/{OperationNo}", orderNo, operationNo);
                return false;
            }
        }
    }
}
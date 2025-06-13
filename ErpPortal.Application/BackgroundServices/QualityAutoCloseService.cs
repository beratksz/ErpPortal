using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ErpPortal.Application.Interfaces.Repositories;
using ErpPortal.Application.Interfaces.Services;

namespace ErpPortal.Application.BackgroundServices
{
    /// <summary>
    ///   Periodically checks operations flagged IsAwaitingQuality and tries to close them on IFS
    ///   once MRB / disposition is completed.
    ///   Runs every 10 minutes.
    /// </summary>
    public class QualityAutoCloseService : BackgroundService
    {
        private readonly IShopOrderOperationRepository _repo;
        private readonly IShopOrderApiService _api;
        private readonly ILogger<QualityAutoCloseService> _logger;
        private const int DelayMinutes = 10;

        public QualityAutoCloseService(IShopOrderOperationRepository repo,
                                        IShopOrderApiService api,
                                        ILogger<QualityAutoCloseService> logger)
        {
            _repo = repo;
            _api = api;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("QualityAutoCloseService started");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var awaitingOps = await _repo.GetAwaitingQualityAsync();
                    foreach (var op in awaitingOps)
                    {
                        if (stoppingToken.IsCancellationRequested) break;
                        try
                        {
                            var payload = new Dictionary<string, object>
                            {
                                { "OperStatusCode", "Closed" },
                                { "OpFinishDate", DateTime.UtcNow },
                                { "ReleaseNo", op.ReleaseNo },
                                { "SequenceNo", op.SequenceNo }
                            };

                            var ok = await _api.PatchOperationDetailsAsync(op.OrderNo, op.OperationNo, payload, op.GetEtag());
                            if (ok)
                            {
                                op.Status = "COMPLETED";
                                op.OperStatusCode = "Closed";
                                op.IsAwaitingQuality = false;
                                op.EndTime = DateTime.UtcNow;
                                op.OpFinishDate = op.EndTime.Value;
                                await _repo.UpdateAsync(op);
                                _logger.LogInformation("Operation {OrderNo}/{OpNo} auto-closed after quality done", op.OrderNo, op.OperationNo);
                            }
                        }
                        catch (Exception exOp)
                        {
                            _logger.LogError(exOp, "Auto-close attempt failed for {OrderNo}/{OpNo}", op.OrderNo, op.OperationNo);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "QualityAutoCloseService cycle failed");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(DelayMinutes), stoppingToken);
                }
                catch (TaskCanceledException) { }
            }
            _logger.LogInformation("QualityAutoCloseService stopped");
        }
    }
} 
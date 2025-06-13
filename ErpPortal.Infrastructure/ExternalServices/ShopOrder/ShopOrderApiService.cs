using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using ErpPortal.Application.Interfaces.Services;
using ErpPortal.Application.Models.ShopOrder;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace ErpPortal.Infrastructure.ExternalServices.ShopOrder
{
    public class ODataResponse<T>
    {
        [JsonPropertyName("value")]
        public List<T> Value { get; set; } = new List<T>();
    }

    public class ShopOrderApiService : IfsApiBase, IShopOrderApiService
    {
        private readonly ILogger<ShopOrderApiService> _logger;

        public ShopOrderApiService(HttpClient httpClient, IConfiguration configuration, ILogger<ShopOrderApiService> logger)
            : base(httpClient, configuration)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<ShopOrderDto>> GetShopOrdersAsync(string workCenterCode)
        {
            var filter = $"WorkCenterNo eq '{HttpUtility.UrlEncode(workCenterCode)}'";
            var endpoint = $"ShopOrderOperationsHandling.svc/ShopOrderOperations?$filter={filter}";
            
            var response = await GetAsync<ODataResponse<ShopOrderDto>>(endpoint);
            return response.Value;
        }

        public async Task<ShopOrderDetailsDto> GetShopOrderDetailsAsync(string orderNo)
        {
            var endpoint = $"/shoporders/{orderNo}";
            return await GetAsync<ShopOrderDetailsDto>(endpoint);
        }

        public async Task<ShopOrderOperationDto> UpdateOperationStatusAsync(string orderNo, string operationNo, OperationStatusUpdateDto updateDto)
        {
            // IFS OData API'si PATCH için ETag ve tam nesne gerektirir.
            // 1. Önce operasyonun mevcut halini ve ETag'ini al.
            // Not: IFS API'si ReleaseNo ve SequenceNo gibi ek anahtarlar gerektirebilir.
            // Bunlar sabit ise hard-code edilebilir veya operasyon verisinden gelmelidir.
            // Şimdilik '*' varsayıyoruz.
            var releaseNo = "*";
            var sequenceNo = "*";
            var getEndpoint = $"ShopOrderOperationsHandling.svc/ShopOrderOperations(OrderNo='{orderNo}',ReleaseNo='{releaseNo}',SequenceNo='{sequenceNo}',OperationNo={operationNo})";
            
            var (currentOperation, etag) = await GetWithETagAsync<JsonElement>(getEndpoint);

            if (string.IsNullOrEmpty(etag))
            {
                throw new HttpRequestException("Operasyon güncellenemedi: ETag alınamadı.");
            }

            // 2. Minimal payload oluştur
            var payload = new Dictionary<string, object>();
            string operCode = updateDto.Status switch
            {
                "STARTED" => "InProcess",
                "STOPPED" => "Interruption",
                _ => updateDto.Status
            };
            payload["OperStatusCode"] = operCode;
            if (!string.IsNullOrEmpty(updateDto.Notes))
                payload["NoteText"] = updateDto.Notes;

            // 3. Güncellenmiş tam nesneyi PATCH isteği için gerçek anahtarları kullan.
            var realReleaseNo = payload.ContainsKey("ReleaseNo") ? payload["ReleaseNo"]?.ToString() ?? releaseNo : releaseNo;
            var realSequenceNo = payload.ContainsKey("SequenceNo") ? payload["SequenceNo"]?.ToString() ?? sequenceNo : sequenceNo;

            var patchEndpoint = ComposeOperationKey(orderNo, realReleaseNo, realSequenceNo, int.Parse(operationNo));
            return await PatchAsync<ShopOrderOperationDto>(patchEndpoint, payload, etag);
        }

        public async Task<bool> PatchOperationStatusAsync(string orderNo, int operationNo, string status, string? reason, string releaseNo = "*", string sequenceNo = "*", string? etag = null)
        {
            var endpoint = ComposeOperationKey(orderNo, releaseNo, sequenceNo, operationNo);
            // IFS entity alanı OperStatusCode, gelen status (STARTED, STOPPED vb.) işletme kodlarına çevrilir
            var operCode = status.ToUpperInvariant() switch
            {
                "STARTED" => "InProcess",
                "STOPPED" => "Interruption",
                "CLOSED" or "COMPLETED" => "Closed",
                _ => status
            };

            var payload = new Dictionary<string, object> { { "OperStatusCode", operCode } };
            // IFS OData servisinde duraklatma gerekçesi NoteText alanına yazılır
            if (!string.IsNullOrEmpty(reason))
                payload.Add("NoteText", reason);
            // Always use If-Match: * for status updates
            try
            {
                await PatchAsync<JsonElement>(endpoint, payload, "*");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PATCH isteği başarısız oldu. Endpoint: {Endpoint}, Payload: {Payload}", endpoint, JsonSerializer.Serialize(payload));
                return false;
            }
        }

        public async Task<bool> ResumeOperationAsync(string orderNo, int operationNo, string releaseNo = "*", string sequenceNo = "*")
        {
            var endpoint = ComposeOperationKey(orderNo, releaseNo, sequenceNo, operationNo);
            var payload = new Dictionary<string, object> { { "Status", "InProcess" } };
            try
            {
                await PatchAsync<JsonElement>(endpoint, payload, "*");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Operasyon devam ettirme isteği başarısız oldu. Endpoint: {Endpoint}, Payload: {Payload}", endpoint, JsonSerializer.Serialize(payload));
                return false;
            }
        }

        public async Task<bool> PatchOperationDetailsAsync(string orderNo, int operationNo, IDictionary<string, object> patchData, string? etag = null)
        {
            // Eğer patchData içinde Status varsa öncelikle UpdateOperationStatusAsync kullan.
            if (patchData.TryGetValue("Status", out var statusObj) && statusObj is string statusStr)
            {
                var rn = patchData.TryGetValue("ReleaseNo", out var rnTmp) ? rnTmp?.ToString() ?? "*" : "*";
                var sn = patchData.TryGetValue("SequenceNo", out var snTmp) ? snTmp?.ToString() ?? "*" : "*";
                // Status güncellemesi minimal payload ile
                await PatchOperationStatusAsync(orderNo, operationNo, statusStr, null, rn, sn, null);
            }
            
            // Tam nesneyi IFS'e PATCH etmek için mevcut kaydı ve ETag'i al
            string releaseNo = patchData.TryGetValue("ReleaseNo", out var rnObj) ? rnObj?.ToString() ?? "*" : "*";
            string sequenceNo = patchData.TryGetValue("SequenceNo", out var snObj) ? snObj?.ToString() ?? "*" : "*";

            var endpoint = ComposeOperationKey(orderNo, releaseNo, sequenceNo, operationNo);

            // --- NEW: Remove key fields before PATCH ---
            patchData.Remove("ReleaseNo");
            patchData.Remove("SequenceNo");
            // ------------------------------------------------

            try
            {
                // Her zaman güncel ETag al
                var (_, currentEtag) = await GetWithETagAsync<JsonElement>(endpoint);
                etag = currentEtag;

                await PatchAsync<JsonElement>(endpoint, patchData, etag!);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<ErpPortal.Domain.Entities.ShopOrderOperation>> GetOperationsAsync(string workCenterCode)
        {
            // Mevcut DTO listesini çekip domain nesnesine dönüştür.
            var dtos = await GetShopOrdersAsync(workCenterCode);
            var result = new List<ErpPortal.Domain.Entities.ShopOrderOperation>();
            foreach (var dto in dtos)
            {
                var op = new ErpPortal.Domain.Entities.ShopOrderOperation
                {
                    OrderNo = dto.OrderNo,
                    OperationNo = dto.OperationNo,
                    WorkCenterCode = dto.WorkCenterNo,
                    WorkCenterNo = dto.WorkCenterNo,
                    OperationDescription = dto.OperationDescription,
                    OperStatusCode = dto.Status,
                    QtyComplete = dto.QtyComplete,
                    QtyScrapped = dto.QtyScrapped,
                    EfficiencyFactor = dto.EfficiencyFactor,
                    MachRunFactor = dto.MachRunFactor,
                    MachSetupTime = dto.MachSetupTime,
                    MoveTime = dto.MoveTime,
                    QueueTime = dto.QueueTime,
                    LaborRunFactor = dto.LaborRunFactor,
                    LaborSetupTime = dto.LaborSetupTime,
                    OpSequenceNo = dto.OpSequenceNo,
                    RevisedQtyDue = dto.RevisedQtyDue,
                    PartNo = dto.PartNo,
                    PartDescription = dto.PartDescription,
                    OpStartDate = dto.OpStartDate ?? DateTime.MinValue,
                    OpFinishDate = dto.OpFinishDate ?? DateTime.MinValue,
                    ETag = dto.ETag,
                    Contract = string.IsNullOrWhiteSpace(dto.Contract) ? "" : dto.Contract,
                    ReleaseNo = dto.ReleaseNo,
                    SequenceNo = dto.SequenceNo
                };
                // RevisedQtyDue, EfficiencyFactor vb. DTO'da yok; gerekirse varsayılan.
                result.Add(op);
            }
            return result;
        }

        // Utility to compose OData key depending on optional release/sequence values
        private static string ComposeOperationKey(string orderNo, string releaseNo, string sequenceNo, int operationNo)
        {
            var parts = new List<string> { $"OrderNo='{orderNo}'", $"ReleaseNo='{releaseNo}'", $"SequenceNo='{sequenceNo}'" };
            parts.Add($"OperationNo={operationNo}");
            return $"ShopOrderOperationsHandling.svc/ShopOrderOperations({string.Join(",", parts)})";
        }
    }
} 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ErpPortal.Application.Interfaces.Services;
using ErpPortal.Application.Models.Quality;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ErpPortal.Infrastructure.ExternalServices;

namespace ErpPortal.Infrastructure.ExternalServices.Quality
{
    internal class NonConformanceApiService : IfsApiBase, INonConformanceApiService
    {
        private readonly ILogger<NonConformanceApiService> _logger;
        private const string ServiceRoot = "NonConformanceHandling.svc";

        public NonConformanceApiService(HttpClient httpClient, IConfiguration configuration, ILogger<NonConformanceApiService> logger)
            : base(httpClient, configuration)
        {
            _logger = logger;
        }

        private class ODataResponse<T>
        {
            public List<T> Value { get; set; } = new();
        }

        public async Task<IEnumerable<NcrDto>> GetOpenNcrsAsync(string orderNo, int operationNo)
        {
            // Özel alan isimleri Cf_Is_Emri_No ve Cf_Operation_No
            var filter = $"Objstate ne 'Closed' and Cf_Is_Emri_No eq '{orderNo}' and Cf_Operation_No eq '{operationNo}'";
            var endpoint = $"{ServiceRoot}/NonConformanceReports?$filter={Uri.EscapeDataString(filter)}";
            try
            {
                var data = await GetAsync<ODataResponse<NcrDto>>(endpoint);
                return data.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NCR sorgusu başarısız. Endpoint: {Endpoint}", endpoint);
                return Enumerable.Empty<NcrDto>();
            }
        }

        public async Task<bool> CloseNcrAsync(string ncrNo, string disposition, string? notes, string? closedBy = null)
        {
            var endpoint = $"{ServiceRoot}/NonConformanceReports(NcrNo='{ncrNo}')";
            var payload = new Dictionary<string, object>
            {
                { "Objstate", "Closed" },
                { "CompletionDate", DateTime.UtcNow },
                { "Disposition", disposition }
            };
            if (!string.IsNullOrEmpty(notes)) payload["Notes"] = notes;
            if (!string.IsNullOrEmpty(closedBy)) payload["ResponsiblePersonId"] = closedBy;

            try
            {
                await PatchAsync<JsonElement>(endpoint, payload, "*");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NCR kapatma başarısız. Endpoint: {Endpoint}, Payload: {Payload}", endpoint, JsonSerializer.Serialize(payload));
                return false;
            }
        }

        public async Task<string?> CreateNcrAsync(string orderNo, int operationNo, decimal scrapQty, string description)
        {
            var endpoint = $"{ServiceRoot}/NonConformanceReports";

            var payload = new Dictionary<string, object>
            {
                { "Description", description },
                { "Company", _configuration["IfsApi:Company"] ?? "MZD" },
                { "Contract", _configuration["IfsApi:Contract"] ?? "MZDM" },
                { "NonconformanceCode", _configuration["IfsApi:NcrCode"] ?? "SHOP" },
                { "SeverityId", _configuration["IfsApi:SeverityId"] ?? "02" },
                { "TargetCompletionDate", DateTime.UtcNow.AddDays(7) },
                { "Cf_Is_Emri_No", orderNo },
                { "Cf_Operation_No", operationNo.ToString() },
                { "Cf_Hurda_Mik", scrapQty }
            };

            try
            {
                var result = await PostAsync<JsonElement>(endpoint, payload);
                return result.TryGetProperty("NcrNo", out var ncrProp) ? ncrProp.GetString() : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NCR oluşturma başarısız. Endpoint: {Endpoint}", endpoint);
                return null;
            }
        }
    }
} 
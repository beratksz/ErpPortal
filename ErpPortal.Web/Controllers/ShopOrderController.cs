using ErpPortal.Application.Interfaces.Services;
using ErpPortal.Application.Models.ShopOrder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ErpPortal.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ShopOrderController : ControllerBase
    {
        private readonly IShopOrderService _shopOrderService;
        private readonly ILogger<ShopOrderController> _logger;

        public ShopOrderController(
            IShopOrderService shopOrderService,
            ILogger<ShopOrderController> logger)
        {
            _shopOrderService = shopOrderService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetShopOrders([FromQuery] string? workCenterCode)
        {
            try
            {
                if (string.IsNullOrEmpty(workCenterCode))
                {
                    return Ok(new List<object>());
                }

                var orders = await _shopOrderService.GetOperationsForWorkCenterAsync(workCenterCode);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shop orders for work center {WorkCenterCode}", workCenterCode);
                return StatusCode(500, new { message = "Shop order'ları alınırken hata oluştu" });
            }
        }

        [HttpPost("{orderNo}/operations/{operationNo}/start")]
        public async Task<IActionResult> StartOperation(string orderNo, int operationNo)
        {
            try
            {
                var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
                var result = await _shopOrderService.StartOperationAsync(orderNo, operationNo, userName);
                
                if (!result)
                {
                    return BadRequest(new { message = "Operasyon başlatılamadı." });
                }

                return Ok(new { message = "Operasyon başarıyla başlatıldı." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting operation {OrderNo}/{OperationNo}", orderNo, operationNo);
                return StatusCode(500, new { message = "Operasyon başlatılırken bir hata oluştu." });
            }
        }

        [HttpPost("{orderNo}/operations/{operationNo}/stop")]
        public async Task<IActionResult> StopOperation(string orderNo, int operationNo, [FromBody] StopOperationRequest request)
        {
            try
            {
                var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
                var result = await _shopOrderService.StopOperationAsync(orderNo, operationNo, userName, request.Reason, request.QuantityCompleted, request.QuantityScrapped);

                if (!result)
                {
                    return BadRequest(new { message = "Operasyon durdurulamadı." });
                }

                return Ok(new { message = "Operasyon başarıyla durduruldu." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping operation {OrderNo}/{OperationNo}", orderNo, operationNo);
                return StatusCode(500, new { message = "Operasyon durdurulurken bir hata oluştu." });
            }
        }
    }

    public class StopOperationRequest
    {
        public string? Reason { get; set; }
        public decimal QuantityCompleted { get; set; }
        public decimal QuantityScrapped { get; set; }
    }
}
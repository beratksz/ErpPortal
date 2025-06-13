using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ErpPortal.Infrastructure.Data;
using ErpPortal.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ErpPortal.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemController : ControllerBase
    {
        private readonly ErpPortalDbContext _context;
        private readonly IShopOrderApiService _apiService;
        private readonly ILogger<SystemController> _logger;

        public SystemController(ErpPortalDbContext context, IShopOrderApiService apiService, ILogger<SystemController> logger)
        {
            _context = context;
            _apiService = apiService;
            _logger = logger;
        }

        public record HealthStatus(DateTime Timestamp, bool Database, bool IfsApi, string? Error);

        [HttpGet("health")]
        public async Task<IActionResult> Health()
        {
            var dbOk = await _context.Database.CanConnectAsync();
            bool apiOk = false;
            string? error = null;

            try
            {
                var list = await _apiService.GetShopOrdersAsync("");
                apiOk = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "IFS API health check failed");
                error = ex.Message;
            }

            var status = new HealthStatus(DateTime.UtcNow, dbOk, apiOk, error);
            return Ok(status);
        }
    }
} 
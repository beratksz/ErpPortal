using ErpPortal.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace ErpPortal.Web.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(ErpPortal.Web.Filters.RequireQualityWorkCenterAttribute))]
    public class QualityController : Controller
    {
        private readonly IQualityService _qualityService;
        private readonly ILogger<QualityController> _logger;

        public QualityController(IQualityService qualityService, ILogger<QualityController> logger)
        {
            _qualityService = qualityService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var awaiting = await _qualityService.GetAwaitingQualityOperationsAsync();
            return View(awaiting);
        }

        // API style endpoint for AJAX
        [HttpGet("api/quality/pending")]
        public async Task<IActionResult> GetPending()
        {
            var awaiting = await _qualityService.GetAwaitingQualityOperationsAsync();
            return Ok(awaiting);
        }

        public record ApproveDto([Required] string Disposition, string? Notes);

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("api/quality/{orderNo}/{operationNo}/approve")]
        public async Task<IActionResult> Approve(string orderNo, int operationNo, [FromBody] ApproveDto dto)
        {
            var userName = User.Identity?.Name ?? "unknown";
            var ok = await _qualityService.ApproveAsync(orderNo, operationNo, userName, dto.Disposition, dto.Notes);
            if (ok) return Ok(new { success = true });
            return BadRequest(new { success = false });
        }
    }
} 
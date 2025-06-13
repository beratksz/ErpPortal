using ErpPortal.Application.Interfaces.Services;
using ErpPortal.Web.Models; // View modelleri için (ReportCompletionViewModel, StopOperationViewModel)
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using ErpPortal.Infrastructure.Data;
using ErpPortal.Application.Models.ShopOrder;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace ErpPortal.Web.Controllers
{
    public class ShopOrderOperationsController : Controller
    {
        private readonly ErpPortalDbContext _context;
        private readonly IShopOrderService _shopOrderService;
        private readonly IUserService _userService;
        private readonly ILogger<ShopOrderOperationsController> _logger;

        public ShopOrderOperationsController(ErpPortalDbContext context, IShopOrderService shopOrderService, IUserService userService, ILogger<ShopOrderOperationsController> logger)
        {
            _context = context;
            _shopOrderService = shopOrderService;
            _userService = userService;
            _logger = logger;
        }

        private bool IsUserLoggedIn(out string? workCenterCode, out string? userName)
        {
            workCenterCode = HttpContext.Session.GetString("WorkCenterCode");
            userName = HttpContext.Session.GetString("UserName");
            return !string.IsNullOrEmpty(workCenterCode) && !string.IsNullOrEmpty(userName);
        }

        private static T? GetPropertyOrDefault<T>(JsonElement json, string prop)
        {
            if (json.TryGetProperty(prop, out var el))
            {
                try
                {
                    if (typeof(T) == typeof(string) && el.ValueKind == JsonValueKind.String)
                        return (T)(object)el.GetString();
                    if (typeof(T) == typeof(decimal) && el.ValueKind == JsonValueKind.Number)
                        return (T)(object)el.GetDecimal();
                    if (typeof(T) == typeof(int) && el.ValueKind == JsonValueKind.Number)
                        return (T)(object)el.GetInt32();
                    if (typeof(T) == typeof(bool) && (el.ValueKind == JsonValueKind.True || el.ValueKind == JsonValueKind.False))
                        return (T)(object)el.GetBoolean();
                }
                catch { }
            }
            return default;
        }

        private string GetCurrentUserName()
        {
            var uname = HttpContext.Session.GetString("UserName");
            if (!string.IsNullOrEmpty(uname)) return uname!;

            uname = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return string.IsNullOrEmpty(uname) ? "Unknown" : uname;
        }

        // API Endpoints for frontend
        [HttpGet]
        [Route("api/shoporder/operations/{workCenter}")]
        public async Task<IActionResult> GetOperationsForWorkCenter(string workCenter)
        {
            if (!IsUserLoggedIn(out _, out var userName)) return Unauthorized();
            
            try
            {
                var operations = await _shopOrderService.GetOperationsForWorkCenterAsync(workCenter);
                return Json(operations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching operations for work center {WorkCenter}", workCenter);
                return StatusCode(500, "An error occurred while fetching operations");
            }
        }

        [HttpGet]
        [Route("api/shoporder/operation/{orderNo}/{opNo}")]
        public async Task<IActionResult> GetOperationDetail(string orderNo, int opNo)
        {
            if (!IsUserLoggedIn(out _, out var userName)) return Unauthorized();
            
            try
            {
                var operation = await _shopOrderService.GetOperationDetailsAsync(orderNo, opNo);
                if (operation == null)
                    return NotFound();
                    
                return Json(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching operation detail for {OrderNo}/{OpNo}", orderNo, opNo);
                return StatusCode(500, "An error occurred while fetching operation detail");
            }
        }

        [HttpPost]
        [Route("api/shoporder/operation/{orderNo}/{opNo}/{opAction}")]
        public async Task<IActionResult> UpdateOperationStatus(string orderNo, int opNo, string opAction, [FromBody] JsonElement? body)
        {
            if (!IsUserLoggedIn(out _, out var userName)) return Unauthorized();
            
            try
            {
                bool success = false;
                decimal qtyCompleted = 0, qtyScrap = 0;
                string? reason = null;
                if (body != null)
                {
                    try
                    {
                        qtyCompleted = GetPropertyOrDefault<decimal>(body.Value, "quantityCompleted");
                        qtyScrap = GetPropertyOrDefault<decimal>(body.Value, "quantityScrapped");
                        reason = GetPropertyOrDefault<string>(body.Value, "reason");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error parsing body");
                    }
                }

                switch (opAction.ToLower())
                {
                    case "start":
                        success = await _shopOrderService.StartOperationAsync(orderNo, opNo, userName!);
                        break;
                    case "stop":
                        success = await _shopOrderService.StopOperationAsync(orderNo, opNo, userName!, reason, qtyCompleted, qtyScrap);
                        break;
                    case "complete":
                        success = await _shopOrderService.ReportCompletionAsync(orderNo, opNo, userName!, qtyCompleted, qtyScrap);
                        break;
                    default:
                        return BadRequest("Invalid action");
                }

                if (success)
                    return Ok();
                else
                    return BadRequest("Operation could not be updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating operation status for {OrderNo}/{OpNo}", orderNo, opNo);
                return StatusCode(500, "An error occurred while updating operation status");
            }
        }

        public async Task<IActionResult> Index()
        {
            if (!IsUserLoggedIn(out var workCenterCode, out var userName))
            {
                return RedirectToAction("Login", "Account");
            }

            _logger.LogInformation("Fetching operations for WC: {WorkCenterCode}, User: {UserName}", workCenterCode, userName);
            ViewBag.WorkCenter = workCenterCode;
            ViewBag.UserName = userName;

            // Kullanıcının sahip olduğu iş merkezleri listesi
            var user = await _userService.GetUserByUsernameAsync(userName!);
            ViewBag.WorkCenters = user?.UserWorkCenters.Select(uwc => uwc.WorkCenter).ToList();

            var operations = await _shopOrderService.GetOperationsForWorkCenterAsync(workCenterCode!); // workCenterCode null olamaz (IsUserLoggedIn kontrolü)
            return View(operations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartOperation(string orderNo, int operationNo)
        {
            if (!IsUserLoggedIn(out _, out var userName)) return Unauthorized();
            if (string.IsNullOrEmpty(orderNo) || operationNo <= 0) return BadRequest("Invalid operation identifiers.");

            _logger.LogInformation("User {UserName} attempting to START OrderNo: {OrderNo}, OpNo: {OperationNo}", userName, orderNo, operationNo);
            var success = await _shopOrderService.StartOperationAsync(orderNo, operationNo, userName!);
            if (success)
                TempData["SuccessMessage"] = $"Operasyon ({orderNo}/{operationNo}) başarıyla başlatıldı.";
            else
                TempData["ErrorMessage"] = $"Operasyon ({orderNo}/{operationNo}) başlatılamadı veya zaten başlamış.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StopOperation(StopOperationViewModel model)
        {
            if (!IsUserLoggedIn(out _, out var userName)) return Unauthorized();
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Durdurma işlemi için geçerli veri girilmedi.";
                // Hata durumunda Index'e geri dönerken modeldeki hataları da göstermek için
                // Index view'ına bu modelin hatalarını taşıyacak bir yapı kurulabilir veya
                // direkt Index'e yönlendirilip genel bir hata mesajı gösterilebilir.
                // Şimdilik basitçe Index'e yönlendiriyoruz.
                return RedirectToAction(nameof(Index));
            }

            _logger.LogInformation("User {UserName} attempting to STOP OrderNo: {OrderNo}, OpNo: {OperationNo} with reason: {Reason}", userName, model.OrderNo, model.OperationNo, model.Reason);
            var success = await _shopOrderService.StopOperationAsync(model.OrderNo, model.OperationNo, userName!, model.Reason, 0, 0);
            if (success)
                TempData["SuccessMessage"] = $"Operasyon ({model.OrderNo}/{model.OperationNo}) başarıyla durduruldu.";
            else
                TempData["ErrorMessage"] = $"Operasyon ({model.OrderNo}/{model.OperationNo}) durdurulamadı veya uygun durumda değil.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportCompletion(ReportCompletionViewModel model)
        {
            if (!IsUserLoggedIn(out _, out var userName)) return Unauthorized();
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Tamamlama raporu için geçerli veri girilmedi.";
                // Hatalı model durumunda Index'e yönlendirme veya modal'ı tekrar gösterme.
                // Şimdilik Index'e yönlendiriyoruz.
                return RedirectToAction(nameof(Index));
            }

            _logger.LogInformation("User {UserName} attempting to REPORT COMPLETION for OrderNo: {OrderNo}, OpNo: {OperationNo}. QtyComplete: {QtyComplete}, QtyScrapped: {QtyScrapped}",
                userName, model.OrderNo, model.OperationNo, model.QuantityCompleted, model.QuantityScrapped);

            var success = await _shopOrderService.ReportCompletionAsync(model.OrderNo, model.OperationNo, userName!, model.QuantityCompleted, model.QuantityScrapped);
            if (success)
                TempData["SuccessMessage"] = $"Operasyon ({model.OrderNo}/{model.OperationNo}) için tamamlama raporlandı.";
            else
                TempData["ErrorMessage"] = $"Operasyon ({model.OrderNo}/{model.OperationNo}) için tamamlama raporlanamadı veya uygun durumda değil.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SelectWorkCenter(string workCenter)
        {
            if (string.IsNullOrEmpty(workCenter)) return RedirectToAction(nameof(Index));
            HttpContext.Session.SetString("WorkCenterCode", workCenter);
            return RedirectToAction(nameof(Index));
        }

        [HttpPatch("ShopOrderOperations/{orderNo}/{operationNo}/status")]
        public async Task<IActionResult> UpdateOperationStatus(string orderNo, int operationNo, [FromBody] JsonElement body)
        {
            var userName = GetCurrentUserName();
            string? status = null;
            string? reason = null;
            decimal qtyCompleted = 0, qtyScrap = 0;
            try
            {
                if (body.TryGetProperty("status", out var statusEl) && statusEl.ValueKind == JsonValueKind.String)
                    status = statusEl.GetString();
                if (body.TryGetProperty("reason", out var reasonEl) && reasonEl.ValueKind == JsonValueKind.String)
                    reason = reasonEl.GetString();
                if (body.TryGetProperty("quantityCompleted", out var qComp) && qComp.ValueKind == JsonValueKind.Number)
                    qtyCompleted = qComp.GetDecimal();
                if (body.TryGetProperty("quantityScrapped", out var qScrap) && qScrap.ValueKind == JsonValueKind.Number)
                    qtyScrap = qScrap.GetDecimal();
            }
            catch { }

            bool result = false;
            switch ((status ?? string.Empty).ToLower())
            {
                case "started":
                    result = await _shopOrderService.StartOperationAsync(orderNo, operationNo, userName!);
                    break;
                case "stopped":
                    result = await _shopOrderService.StopOperationAsync(orderNo, operationNo, userName!, reason, qtyCompleted, qtyScrap);
                    break;
                case "complete":
                case "completed":
                    result = await _shopOrderService.ReportCompletionAsync(orderNo, operationNo, userName!, qtyCompleted, qtyScrap);
                    break;
                default:
                    return BadRequest("Geçersiz status değeri");
            }
            if (result)
                return Ok();
            return BadRequest("Operasyon durumu güncellenemedi.");
        }

        [HttpPost("ShopOrderOperations/{orderNo}/{operationNo}/resume")]
        public async Task<IActionResult> ResumeOperationApi(string orderNo, int operationNo)
        {
            var userName = GetCurrentUserName();
            var result = await _shopOrderService.ResumeOperationAsync(orderNo, operationNo, userName);
            if (result)
                return Ok(new { success = true });
            return BadRequest(new { success = false, message = "Operasyon devam ettirilemedi." });
        }

        [HttpPost("ShopOrderOperations/{orderNo}/{operationNo}/progress")]
        public async Task<IActionResult> UpdateQuantitiesApi(string orderNo, int operationNo, [FromBody] JsonElement body)
        {
            if (!IsUserLoggedIn(out _, out var userName)) return Unauthorized();

            decimal addedCompleted = 0;
            decimal addedScrapped = 0;
            string? reason = null;
            try
            {
                addedCompleted = GetPropertyOrDefault<decimal>(body, "addedCompleted");
                addedScrapped = GetPropertyOrDefault<decimal>(body, "addedScrapped");
                reason = GetPropertyOrDefault<string>(body, "reason");
            }
            catch { }

            if (addedCompleted <= 0 && addedScrapped <= 0)
            {
                return BadRequest("Eklenen miktar girilmedi.");
            }

            var success = await _shopOrderService.UpdateQuantitiesAsync(orderNo, operationNo, userName!, addedCompleted, addedScrapped, reason);
            if (success)
                return Ok(new { success = true });
            else
                return BadRequest(new { success = false, message = "Güncelleme yapılamadı." });
        }

        // ----------- NEW Razor detail page -----------
        [HttpGet]
        public async Task<IActionResult> Detail(string orderNo, int operationNo)
        {
            if (!IsUserLoggedIn(out _, out var userName))
            {
                return RedirectToAction("Login", "Account");
            }

            var operation = await _shopOrderService.GetOperationDetailsAsync(orderNo, operationNo);
            if (operation == null)
            {
                TempData["ErrorMessage"] = "Operasyon bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.UserName = userName;
            return View(operation);
        }
    }
}

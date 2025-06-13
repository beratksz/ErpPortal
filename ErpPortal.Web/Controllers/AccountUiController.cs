using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Linq;
using ErpPortal.Application.Interfaces.Services;
using ErpPortal.Web.Models;
using System;

namespace ErpPortal.Web.Controllers
{
    public class AccountUiController : Controller
    {
        private readonly IUserService _userService;
        public AccountUiController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("/Account/Login")]
        public IActionResult Login()
        {
            return View("~/Views/Account/Login.cshtml");
        }

        [HttpPost("/Account/Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View("~/Views/Account/Login.cshtml", model);
            var user = await _userService.ValidateUserAsync(model.Username, model.Password);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı veya şifre");
                return View("~/Views/Account/Login.cshtml", model);
            }

            HttpContext.Session.SetString("UserName", user.Username);
            HttpContext.Session.SetString("IsAdmin", user.IsAdmin ? "true" : "false");

            // Work center seçim mantığı
            var wcs = user.UserWorkCenters.Select(uwc => uwc.WorkCenter).ToList();

            if (user.IsAdmin)
            {
                // Admin kullanıcıları Admin alanındaki UsersController Index aksiyonuna yönlendir.
                return RedirectToAction("Index", "Users", new { area = "Admin" });
            }

            if (wcs.Count == 1)
            {
                HttpContext.Session.SetString("WorkCenterCode", wcs[0].Code);
                return RedirectToAction("Index", "ShopOrderOperations");
            }

            // Birden fazla iş merkezi varsa seçim sayfasına yönlendir.
            TempData["Username"] = user.Username;
            return RedirectToAction("SelectWorkCenter");
        }

        [HttpPost("/Account/Logout")]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet("/Account/SelectWorkCenter")]
        public async Task<IActionResult> SelectWorkCenter()
        {
            var username = TempData["Username"] as string ?? HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login");

            var user = await _userService.GetUserByUsernameAsync(username);
            var model = new SelectWorkCenterViewModel
            {
                Username = username,
                WorkCenters = user.UserWorkCenters.Select(uwc => uwc.WorkCenter).ToList()
            };
            return View("~/Views/Account/SelectWorkCenter.cshtml", model);
        }

        [HttpPost("/Account/SelectWorkCenter")]
        [ValidateAntiForgeryToken]
        public IActionResult SelectWorkCenter(string workCenterCode)
        {
            if (string.IsNullOrEmpty(workCenterCode))
            {
                TempData["Error"] = "İş merkezi seçmelisiniz.";
                return RedirectToAction("SelectWorkCenter");
            }
            HttpContext.Session.SetString("WorkCenterCode", workCenterCode);
            return RedirectToAction("Index", "ShopOrderOperations");
        }
    }
} 
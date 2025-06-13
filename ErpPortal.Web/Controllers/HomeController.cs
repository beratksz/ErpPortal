using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ErpPortal.Web.Models;

namespace ErpPortal.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // Eğer oturumda WorkCenter seçiliyse Operasyon listesine gönder; değilse Account/Login'e.
        var wc = HttpContext.Session.GetString("WorkCenterCode");
        if (string.IsNullOrEmpty(wc))
            return RedirectToAction("Login", "AccountUi");
        return RedirectToAction("Index", "ShopOrderOperations");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

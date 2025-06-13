using Microsoft.AspNetCore.Mvc;
using ErpPortal.Application.Interfaces.Services;
using ErpPortal.Web.Models;
using System.Threading.Tasks;

namespace ErpPortal.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class WorkCentersController : Controller
    {
        private readonly IWorkCenterService _service;
        public WorkCentersController(IWorkCenterService service) { _service = service; }
        public async Task<IActionResult> Index()
        {
            var wcs = await _service.GetAllWorkCentersAsync();
            return View(wcs);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new WorkCenterFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkCenterFormViewModel model)
        {
            if(!ModelState.IsValid) return View(model);
            var entity = new ErpPortal.Domain.Entities.WorkCenter{
                Code = model.Code,
                Name = model.Name,
                Description = model.Description,
                IsActive = model.IsActive
            };
            await _service.CreateWorkCenterAsync(entity);
            TempData["SuccessMessage"] = "İş merkezi eklendi";
            return RedirectToAction(nameof(Index));
        }
    }
} 
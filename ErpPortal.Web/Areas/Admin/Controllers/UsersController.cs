using Microsoft.AspNetCore.Mvc;
using ErpPortal.Application.Interfaces.Services;
using ErpPortal.Web.Models;
using System.Threading.Tasks;

namespace ErpPortal.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly IWorkCenterService _workCenterService;
        public UsersController(IUserService userService, IWorkCenterService wcService)
        {
            _userService = userService;
            _workCenterService = wcService;
        }

        [HttpGet("/Admin")]
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        // ---------- Create User ----------
        [HttpGet]
        public IActionResult Create()
        {
            return View(new UserFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormViewModel model)
        {
            if(!ModelState.IsValid) return View(model);

            var entity = new ErpPortal.Domain.Entities.User{
                Username = model.Username,
                FullName = model.FullName,
                Password = model.Password,
                IsAdmin = model.IsAdmin,
                IsActive = model.IsActive,
                CreatedAt = System.DateTime.UtcNow
            };
            await _userService.CreateUserAsync(entity);
            TempData["SuccessMessage"] = "Kullanıcı oluşturuldu";
            return RedirectToAction(nameof(Index));
        }

        // ---------- Assign Work Centers ----------
        [HttpGet]
        public async Task<IActionResult> Assign(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if(user == null) return NotFound();
            var allWc = await _workCenterService.GetAllWorkCentersAsync();
            var model = new AssignWorkCentersViewModel{
                UserId = user.Id,
                Username = user.Username,
                WorkCenters = allWc.Select(wc => new WorkCenterCheckbox{
                    Id = wc.Id,
                    Code = wc.Code,
                    Name = wc.Name,
                    Selected = user.UserWorkCenters.Any(u=>u.WorkCenterId==wc.Id)
                }).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int userId, int[] selectedWorkCenters)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if(user == null) return NotFound();
            var allWc = await _workCenterService.GetAllWorkCentersAsync();
            // Current assignments
            var currentIds = user.UserWorkCenters.Select(u=>u.WorkCenterId).ToHashSet();
            var selectedSet = selectedWorkCenters.ToHashSet();
            // Add new
            foreach(var wc in allWc.Where(wc=> selectedSet.Contains(wc.Id) && !currentIds.Contains(wc.Id)))
                await _userService.AssignWorkCenterAsync(userId,wc.Id);
            // Remove unchecked
            foreach(var wcId in currentIds.Where(id=> !selectedSet.Contains(id)))
                await _userService.RemoveWorkCenterAsync(userId,wcId);

            TempData["SuccessMessage"] = "İş merkezleri güncellendi";
            return RedirectToAction(nameof(Index));
        }
    }
} 
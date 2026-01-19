using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovelLogger.Data;
using NovelLogger.Models;
using NovelLogger.Services;
using NovelLogger.Utility;
using System.Security.Claims;

namespace NovelLogger.Controllers
{
    [Authorize]
    public class NovelController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ISaveChangesService _saveChangesService;

        public NovelController(ApplicationDbContext db, ISaveChangesService saveChangesService)
        {
            _db = db;
            _saveChangesService = saveChangesService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            NovelVM vm = new NovelVM()
            {
                NovelStatusList = NovelStatusStrings.StatusOptions,
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(NovelVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.NovelStatusList = NovelStatusStrings.StatusOptions;

                return View(vm);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Novel novel = new Novel()
            {
                Title = vm.NovelTitle,
                TitleNormalized = StringUtilityMethods.NormalizeTitle(vm.NovelTitle),
                NovelStatus = vm.NovelStatus,
                UserId = userId,
            };

            _db.Novels.Add(novel);

            if (_saveChangesService.TrySave() == SaveResult.Duplicate)
            {
                ModelState.AddModelError(nameof(vm.NovelTitle), "This novel already exists.");
                vm.NovelStatusList = NovelStatusStrings.StatusOptions;
                return View(vm);
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int novelId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Novel? novel = _db.Novels.Where(u => u.UserId == userId && u.Id == novelId).FirstOrDefault();

            if (novel == null)
            {
                return NotFound();
            }

            NovelVM vm = new NovelVM()
            {
                NovelTitle = novel.Title,
                NovelStatus = novel.NovelStatus,
                NovelStatusList = NovelStatusStrings.StatusOptions,
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(NovelVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.NovelStatusList = NovelStatusStrings.StatusOptions;

                return View(vm);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Novel? novel = _db.Novels.Where(u => u.UserId == userId && u.Id == vm.NovelId).FirstOrDefault();

            if(novel == null)
            {
                return NotFound();
            }

            novel.Title = vm.NovelTitle;
            novel.TitleNormalized = StringUtilityMethods.NormalizeTitle(vm.NovelTitle);
            novel.NovelStatus = vm.NovelStatus;

            if (_saveChangesService.TrySave() == SaveResult.Duplicate)
            {
                ModelState.AddModelError(nameof(vm.NovelTitle), "This novel already exists.");
                vm.NovelStatusList = NovelStatusStrings.StatusOptions;
                return View(vm);
            }

            return RedirectToAction(nameof(Index));
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var novels = _db.Novels.Where(u => u.UserId == userId).Select(u => new
            {
                novelTitle = u.Title,
                novelStatus = u.NovelStatus,
                novelId = u.Id,
            }).ToList();

            return Json(new { data = novels });
        }

        [HttpDelete]
        public IActionResult Delete(int? novelId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Novel? novel = _db.Novels.Where(u => u.UserId == userId && u.Id == novelId).FirstOrDefault();

            if (novel == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _db.Novels.Remove(novel);
            _db.SaveChanges();

            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}

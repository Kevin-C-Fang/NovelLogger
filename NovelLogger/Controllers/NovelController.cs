using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovelLogger.Data;
using NovelLogger.Models.DTOs;
using NovelLogger.Models.Entities;
using NovelLogger.Models.ViewModels;
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

            // TODO: Convert VM to dto and pass into service to be saved to database.
            var createNovelDto = new CreateNovelDto
            {
                NovelTitle = vm.NovelTitle,
                NovelStatus = vm.NovelStatus
            };

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            Novel novel = new Novel()
            {
                Title = createNovelDto.NovelTitle,
                TitleNormalized = StringUtilityMethods.NormalizeTitle(createNovelDto.NovelTitle),
                NovelStatus = createNovelDto.NovelStatus,
                UserId = userId,
            };

            _db.Novels.Add(novel);

            // TODO: Incorporate the save changes service into new service and return boolean signaling whether save went through.
            if (_saveChangesService.TrySave() == SaveResult.NovelTitleNormDuplicate)
            {
                ModelState.AddModelError(nameof(vm.NovelTitle), "A novel with this title already exists. If you just submitted this form, it may have been created already.");
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

            // TODO: Grab entity from database, convert to dto, grab dto from service and convert to vm to be passed to view.
            var editNovelDto = new EditNovelDto()
            {
                NovelTitle = novel.Title,
                NovelStatus = novel.NovelStatus
            };

            NovelVM vm = new NovelVM()
            {
                NovelTitle = editNovelDto.NovelTitle,
                NovelStatus = editNovelDto.NovelStatus,
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

            // TODO: Convert VM to dto, pass to service, and edit in service and save to database. 
            var editNovelDto = new EditNovelDto
            {
                NovelTitle = vm.NovelTitle,
                NovelStatus = vm.NovelStatus
            };

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Novel? novel = _db.Novels.Where(u => u.UserId == userId && u.Id == vm.NovelId).FirstOrDefault();

            if(novel == null)
            {
                return NotFound();
            }

            novel.Title = vm.NovelTitle;
            novel.TitleNormalized = StringUtilityMethods.NormalizeTitle(vm.NovelTitle);
            novel.NovelStatus = vm.NovelStatus;

            // TODO: Incorporate the save changes service into new service and return boolean signaling whether save went through.
            if (_saveChangesService.TrySave() == SaveResult.NovelTitleNormDuplicate)
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
            // TODO: Grab data from database in service, convert to dto, return and convert again to NovelVM.
            
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
            // TODO: When service is created, add pathway to delete by calling service method.

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

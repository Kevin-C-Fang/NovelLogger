using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovelLogger.Data;
using NovelLogger.Models;
using NovelLogger.Utility;
using System.Security.Claims;

namespace NovelLogger.Controllers
{
    [Authorize]
    public class NovelController : Controller
    {
        private readonly ApplicationDbContext _db;

        public NovelController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            NovelVM vm = new NovelVM()
            {
                NovelStatusList = NovelStatusStrings.All.Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = s,
                    Value = s,
                })
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(NovelVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.NovelStatusList = NovelStatusStrings.All.Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = s,
                    Value = s,
                });

                return View(vm);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Novel novel = new Novel()
            {
                Title = vm.NovelTitle,
                TitleNormalized = StringUtilityMethods.NormalizeTitle(vm.NovelStatus),
                NovelStatus = vm.NovelStatus,
                UserId = userId,
            };

            _db.Novels.Add(novel);
            _db.SaveChanges();

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
                NovelStatusList = NovelStatusStrings.All.Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = s,
                    Value = s,
                })
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(NovelVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.NovelStatusList = NovelStatusStrings.All.Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = s,
                    Value = s,
                });

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

            _db.SaveChanges();

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

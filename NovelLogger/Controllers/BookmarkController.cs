using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
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
    public class BookmarkController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ISaveChangesService _saveChangesService;

        public BookmarkController(ApplicationDbContext db, ISaveChangesService saveChangesService)
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
            BookmarkVM vm = new BookmarkVM()
            {
                NovelStatusList = NovelStatusStrings.StatusOptions,
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BookmarkVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.NovelStatusList = NovelStatusStrings.StatusOptions;
                return View(vm);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            string normalizedTitle = StringUtilityMethods.NormalizeTitle(vm.NovelTitle);

            Bookmark? bookmark = _db.Bookmarks.Where(u => u.UserId == userId && u.Novel.TitleNormalized == normalizedTitle && !u.IsSaved).FirstOrDefault();

            if (bookmark != null)
            {
                _db.Bookmarks.Remove(bookmark);
            }

            Novel? novel = _db.Novels.Where(u => u.UserId == userId && u.TitleNormalized == normalizedTitle).FirstOrDefault();

            if (novel == null)
            {
                novel = new Novel()
                {
                    UserId = userId,
                    Title = vm.NovelTitle,
                    TitleNormalized = normalizedTitle,
                    NovelStatus = vm.NovelStatus,
                };

                _db.Novels.Add(novel);
            }
            else
            {
                novel.NovelStatus = vm.NovelStatus;
            }

            bookmark = new Bookmark()
            {
                UserId = userId,
                Novel = novel,
                Url = vm.Url,
                Notes = vm.Notes,
                IsSaved = vm.IsSaved,
                DateAdded = DateTime.UtcNow
            };

            _db.Bookmarks.Add(bookmark);
            if (_saveChangesService.TrySave() == SaveResult.Duplicate)
            {
                novel = _db.Novels.Single(n => n.UserId == userId && n.TitleNormalized == normalizedTitle);
                bookmark.Novel = novel;
                _db.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewBookmark(int bookmarkId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Bookmark? bookmark = _db.Bookmarks.Where(u => u.UserId == userId && u.Id == bookmarkId).Include(b => b.Novel).FirstOrDefault();

            if (bookmark == null) {
                return NotFound();
            }

            BookmarkVM vm = new BookmarkVM()
            {
                NovelTitle = bookmark.Novel.Title,
                Url = bookmark.Url,
                Notes = bookmark.Notes,
                IsSaved = bookmark.IsSaved,
                NovelStatus = bookmark.Novel.NovelStatus,
            };

            return View(vm);
        }

        public IActionResult Edit(int bookmarkId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Bookmark? bookmark = _db.Bookmarks.Where(u => u.UserId == userId && u.Id == bookmarkId).Include(b => b.Novel).FirstOrDefault();

            if (bookmark == null)
            {
                return NotFound();
            }

            BookmarkVM vm = new BookmarkVM()
            {
                NovelTitle = bookmark.Novel.Title,
                Url = bookmark.Url,
                Notes = bookmark.Notes,
                IsSaved = bookmark.IsSaved,
                BookmarkId = bookmark.Id,
                NovelStatus = bookmark.Novel.NovelStatus,
                NovelStatusList = NovelStatusStrings.StatusOptions,
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BookmarkVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.NovelStatusList = NovelStatusStrings.StatusOptions;

                return View(vm);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            Bookmark? bookmark = _db.Bookmarks.Where(u => u.UserId == userId && u.Id == vm.BookmarkId).Include(b => b.Novel).FirstOrDefault();

            if (bookmark == null)
            {
                return NotFound();
            }

            Novel? novel = _db.Novels.Where(u => u.UserId == userId && u.Id == bookmark.NovelId).FirstOrDefault();

            if (novel == null)
            {
                return NotFound();
            }

            novel.NovelStatus = vm.NovelStatus;

            bookmark.Notes = vm.Notes;
            bookmark.Url = vm.Url;

            if (!bookmark.IsSaved)
            {
                bookmark.IsSaved = vm.IsSaved;
            }

            _db.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var bookmarks = _db.Bookmarks.Where(u => u.UserId == userId).Include(b => b.Novel).Select(u => new
                {
                    novel = new { title = u.Novel.Title },
                    url = u.Url,
                    dateAdded = new
                    {
                        display = u.DateAdded.ToString("MM/dd/yyyy hh:mm:ss tt"),
                        sort = u.DateAdded
                    },
                    hasNotes = !string.IsNullOrEmpty(u.Notes) ? "✓" : "✗",
                    isSaved = u.IsSaved ? "✓" : "✗",
                    bookmarkId = u.Id,
                }).ToList();

            return Json(new { data = bookmarks });
        }

        [HttpGet]
        public IActionResult NovelTitleSuggestions(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Json(null);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string normalized = StringUtilityMethods.NormalizeTitle(text);

            var novelTitles = _db.Novels.Where(u => u.UserId == userId && u.TitleNormalized.Contains(normalized))
                .OrderBy(u=> u.TitleNormalized).Select(n => n.Title).Take(5).ToList();

            return Json(novelTitles);
        }

        [HttpDelete]
        public IActionResult Delete(int? bookmarkId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Bookmark? bookmark = _db.Bookmarks.Where(u => u.UserId == userId && u.Id == bookmarkId).FirstOrDefault();

            if (bookmark == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _db.Bookmarks.Remove(bookmark);
            _db.SaveChanges();

            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}

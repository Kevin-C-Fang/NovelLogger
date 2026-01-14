using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovelLogger.Data;
using NovelLogger.Models;
using System.Security.Claims;

namespace NovelLogger.Controllers
{
    [Authorize]
    public class BookmarkController : Controller
    {
        private readonly ApplicationDbContext _db;

        public BookmarkController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BookmarkVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            string normalizedTitle = NormalizeTitle(vm.NovelTitle);

            Bookmark? bookmark = _db.Bookmarks.Include("Novel").FirstOrDefault(u => u.UserId == userId && u.Novel.TitleNormalized == normalizedTitle && !u.IsSaved);

            if (bookmark != null)
            {
                _db.Bookmarks.Remove(bookmark);
                _db.SaveChanges();
            }

            Novel? novel = _db.Novels.FirstOrDefault(u => u.UserId == userId && u.TitleNormalized == normalizedTitle);

            if (novel == null)
            {
                novel = new Novel()
                {
                    UserId = userId,
                    Title = vm.NovelTitle,
                    TitleNormalized = normalizedTitle
                };

                _db.Novels.Add(novel);
                _db.SaveChanges();
            }

            bookmark = new Bookmark()
            {
                UserId = userId,
                NovelId = novel.Id,
                Url = vm.Url,
                Notes = vm.Notes,
                IsSaved = vm.IsSaved,
                DateAdded = DateTime.UtcNow
            };

            _db.Bookmarks.Add(bookmark);
            _db.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewBookmark(int bookmarkId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Bookmark? bookmark = _db.Bookmarks.Where(u => u.UserId == userId && u.Id == bookmarkId).Include("Novel").FirstOrDefault();

            if (bookmark == null) {
                return NotFound();
            }

            BookmarkVM vm = new BookmarkVM()
            {
                NovelTitle = bookmark.Novel.Title,
                Url = bookmark.Url,
                Notes = bookmark.Notes,
                IsSaved = bookmark.IsSaved,
            };

            return View(vm);
        }

        public IActionResult Edit(int bookmarkId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Bookmark? bookmark = _db.Bookmarks.Where(u => u.UserId == userId && u.Id == bookmarkId).Include("Novel").FirstOrDefault();

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
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BookmarkVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Bookmark? bookmark = _db.Bookmarks.Where(u => u.UserId == userId && u.Id == vm.BookmarkId).Include("Novel").FirstOrDefault();

            if (bookmark == null)
            {
                return NotFound();
            }

            bookmark.Novel.Title = vm.NovelTitle;
            bookmark.Novel.TitleNormalized = NormalizeTitle(vm.NovelTitle);
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

            var bookmarks = _db.Bookmarks.Where(u => u.UserId == userId).Include("Novel").Select(u => new
                {
                    novel = new { title = u.Novel.Title },
                    url = u.Url,
                    dateAdded = new
                    {
                        display = u.DateAdded.ToString("MM/dd/yyyy hh:mm:ss tt"),
                        sort = u.DateAdded
                    },
                    hasNotes = !string.IsNullOrEmpty(u.Notes) ? "\u2714" : "\u2716",
                    isSaved = u.IsSaved ? "\u2714" : "\u2716",
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
            string normalized = NormalizeTitle(text);

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

        public string NormalizeTitle(string title)
        {
            return string.Join(' ', title.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }
    }
}

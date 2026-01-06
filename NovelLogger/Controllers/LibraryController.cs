using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovelLogger.Data;
using NovelLogger.Models;
using System.Security.Claims;

namespace NovelLogger.Controllers
{
    [Authorize]
    public class LibraryController : Controller
    {
        private readonly ApplicationDbContext _db;

        public LibraryController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            // TODO: Should novel.title be autofilled with prior novels relevant to the user?
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateBookmarkVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            string normalizedTitle = NormalizeTitle(vm.NovelTitle);

            Novel novel = _db.Novels.FirstOrDefault(u => u.UserId == userId && u.TitleNormalized == normalizedTitle);

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

            Bookmark bookmark = new Bookmark()
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

        public string NormalizeTitle(string title)
        {
            return string.Join(' ', title.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }
    }
}

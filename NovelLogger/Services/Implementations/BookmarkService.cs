using Humanizer;
using Microsoft.EntityFrameworkCore;
using NovelLogger.Data;
using NovelLogger.Models.DTOs;
using NovelLogger.Models.Entities;
using NovelLogger.Models.ViewModels;
using NovelLogger.Services.Interfaces;
using NovelLogger.Utility;
using System.Security.Claims;

namespace NovelLogger.Services.Implementations
{
    public class BookmarkService : IBookmarkService
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INovelService _novelService;
        private readonly ISaveChangesService _saveChangesService;

        public BookmarkService(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor, INovelService novelService, ISaveChangesService saveChangesService)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _novelService = novelService;
            _saveChangesService = saveChangesService;
        }

        public ServiceResult CreateBookmark(CreateBookmarkDto dto)
        {
            ServiceResult createNovelResult = HandleCreateNovelFlow(dto);

            if(createNovelResult != ServiceResult.Success)
            {
                return createNovelResult;
            }

            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            RemoveLatestUnsavedBookmark(userId, dto.TitleNormalized);

            Novel? novel = _db.Novels.Where(u => u.UserId == userId && u.TitleNormalized == dto.TitleNormalized).FirstOrDefault();

            Bookmark bookmark = new Bookmark()
            {
                UserId = userId,
                NovelId = novel.Id,
                Url = dto.Url,
                Notes = dto.Notes,
                IsSaved = dto.IsSaved,
                DateAdded = DateTime.UtcNow
            };

            _db.Bookmarks.Add(bookmark);

            // TODO: Incorporate the save changes service into new service and return boolean signaling whether save went through.
            return _saveChangesService.TrySave();
        }

        public ViewBookmarkDto? GetViewBookmarkDto(int bookmarkId)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Bookmark? bookmark = _db.Bookmarks.Where(u => u.UserId == userId && u.Id == bookmarkId).Include(b => b.Novel).FirstOrDefault();

            if (bookmark == null)
            {
                return null;
            }

            return MapViewBookmarkToDto(bookmark);
        }

        public EditBookmarkDto? GetEditBookmarkDto(int bookmarkId)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Bookmark? bookmark = _db.Bookmarks.Where(u => u.UserId == userId && u.Id == bookmarkId).Include(b => b.Novel).FirstOrDefault();

            if (bookmark == null)
            {
                return null;
            }

            return MapEditBookmarkToDto(bookmark);
        }

        public ServiceResult EditBookmark(EditBookmarkDto dto)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Bookmark? bookmark = _db.Bookmarks.Where(u => u.UserId == userId && u.Id == dto.BookmarkId).Include(b => b.Novel).FirstOrDefault();

            if (bookmark == null)
            {
                return ServiceResult.NotFound;
            }

            bookmark.Novel.NovelStatus = dto.NovelStatus;

            bookmark.Notes = dto.Notes;
            bookmark.Url = dto.Url;

            if (!bookmark.IsSaved)
            {
                bookmark.IsSaved = dto.IsSaved;
            }

            return _saveChangesService.TrySave();
        }

        public List<ViewBookmarkDto> GetAllViewBookmarkDto()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var bookmarks = _db.Bookmarks.Where(u => u.UserId == userId).Include(b => b.Novel).Select(u => new ViewBookmarkDto
            {
                NovelTitle = u.Novel.Title,
                NovelStatus = u.Novel.NovelStatus,
                BookmarkId = u.Id,
                Url = u.Url,
                Notes = u.Notes,
                IsSaved = u.IsSaved,
                DateAdded = u.DateAdded,
            }).ToList();

            return bookmarks;
        }

        public ServiceResult DeleteBookmark(int bookmarkId)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Bookmark? bookmark = _db.Bookmarks.Where(u => u.UserId == userId && u.Id == bookmarkId).FirstOrDefault();

            if (bookmark == null)
            {
                return ServiceResult.NotFound;
            }

            _db.Bookmarks.Remove(bookmark);
            return _saveChangesService.TrySave();
        }

        private void RemoveLatestUnsavedBookmark(string? userId, string titleNormlized)
        {
            Bookmark? bookmarkFromDb = _db.Bookmarks.Where(u => u.UserId == userId &&
                u.Novel.TitleNormalized == titleNormlized && !u.IsSaved).FirstOrDefault();

            if (bookmarkFromDb != null)
            {
                _db.Bookmarks.Remove(bookmarkFromDb);
            }
        }

        private ServiceResult HandleCreateNovelFlow(CreateBookmarkDto dto)
        {
            var novelDto = _novelService.GetViewNovelDto(dto.TitleNormalized);

            if (novelDto == null)
            {
                return _novelService.CreateNovel(new CreateNovelDto()
                {
                    NovelTitle = dto.NovelTitle,
                    NovelStatus = dto.NovelStatus,
                });
            }
            else
            {
                return _novelService.EditNovel(new EditNovelDto()
                {
                    NovelTitle = dto.NovelTitle,
                    NovelStatus = dto.NovelStatus,
                    NovelId = novelDto.NovelId
                });
            }
        }

        private ViewBookmarkDto MapViewBookmarkToDto(Bookmark bookmark)
        {
            return new ViewBookmarkDto()
            {
                NovelTitle = bookmark.Novel.Title,
                BookmarkId = bookmark.Id,
                Url = bookmark.Url,
                Notes = bookmark.Notes,
                IsSaved = bookmark.IsSaved,
                NovelStatus = bookmark.Novel.NovelStatus,
            };
        }

        private EditBookmarkDto MapEditBookmarkToDto(Bookmark bookmark)
        {
            return new EditBookmarkDto()
            {
                NovelTitle = bookmark.Novel.Title,
                Url = bookmark.Url,
                Notes = bookmark.Notes,
                IsSaved = bookmark.IsSaved,
                NovelStatus = bookmark.Novel.NovelStatus,
            };
        }
    }
}

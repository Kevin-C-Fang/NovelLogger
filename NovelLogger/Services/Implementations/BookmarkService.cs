using Humanizer;
using Microsoft.EntityFrameworkCore;
using NovelLogger.Data;
using NovelLogger.Data.Repositories;
using NovelLogger.Data.Repositories.IRepositories;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INovelService _novelService;
        private readonly IUnitOfWork _unitOfWork;

        public BookmarkService(IHttpContextAccessor httpContextAccessor, INovelService novelService, IUnitOfWork unitOfWork)
        {
            _httpContextAccessor = httpContextAccessor;
            _novelService = novelService;
            _unitOfWork = unitOfWork;
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

            Novel? novel = _unitOfWork.Novel.GetFirstOrDefault(u => u.UserId == userId && u.TitleNormalized == dto.TitleNormalized);

            Bookmark bookmark = new Bookmark()
            {
                UserId = userId,
                NovelId = novel.Id,
                Url = dto.Url,
                Notes = dto.Notes,
                IsSaved = dto.IsSaved,
                DateAdded = DateTime.UtcNow
            };

            _unitOfWork.Bookmark.Add(bookmark);

            return _unitOfWork.TrySave();
        }

        public ViewBookmarkDto? GetViewBookmarkDto(int bookmarkId)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Bookmark? bookmark = _unitOfWork.Bookmark.GetFirstOrDefault(u => u.UserId == userId && u.Id == bookmarkId, includeProperties: "Novel");

            if (bookmark == null)
            {
                return null;
            }

            return MapViewBookmarkToDto(bookmark);
        }

        public EditBookmarkDto? GetEditBookmarkDto(int bookmarkId)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Bookmark? bookmark = _unitOfWork.Bookmark.GetFirstOrDefault(u => u.UserId == userId && u.Id == bookmarkId, includeProperties: "Novel");

            if (bookmark == null)
            {
                return null;
            }

            return MapEditBookmarkToDto(bookmark);
        }

        public ServiceResult EditBookmark(EditBookmarkDto dto)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Bookmark? bookmark = _unitOfWork.Bookmark.GetFirstOrDefault(u => u.UserId == userId && u.Id == dto.BookmarkId, includeProperties: "Novel", true);

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

            return _unitOfWork.TrySave();
        }

        public List<ViewBookmarkDto> GetAllViewBookmarkDto()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var bookmarks = _unitOfWork.Bookmark.GetAll(u => u.UserId == userId, includeProperties: "Novel").Select(u => new ViewBookmarkDto
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
            Bookmark? bookmark = _unitOfWork.Bookmark.GetFirstOrDefault(u => u.UserId == userId && u.Id == bookmarkId);

            if (bookmark == null)
            {
                return ServiceResult.NotFound;
            }

            _unitOfWork.Bookmark.Remove(bookmark);
            return _unitOfWork.TrySave();
        }

        private void RemoveLatestUnsavedBookmark(string? userId, string titleNormlized)
        {
            Bookmark bookmarkFromDb = _unitOfWork.Bookmark.GetFirstOrDefault(u => u.UserId == userId &&
                u.Novel.TitleNormalized == titleNormlized && !u.IsSaved);

            if (bookmarkFromDb != null)
            {
                _unitOfWork.Bookmark.Remove(bookmarkFromDb);
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
                BookmarkId = bookmark.Id,
                Url = bookmark.Url,
                Notes = bookmark.Notes,
                IsSaved = bookmark.IsSaved,
                NovelStatus = bookmark.Novel.NovelStatus,
            };
        }
    }
}

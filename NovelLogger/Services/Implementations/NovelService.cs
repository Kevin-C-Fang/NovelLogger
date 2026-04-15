using Microsoft.AspNetCore.Mvc;
using NovelLogger.Data;
using NovelLogger.Models.DTOs;
using NovelLogger.Models.Entities;
using NovelLogger.Models.ViewModels;
using NovelLogger.Services.Interfaces;
using NovelLogger.Utility;
using System.Security.Claims;

namespace NovelLogger.Services.Implementations
{
    public class NovelService: INovelService
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISaveChangesService _saveChangesService;

        public NovelService(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor, ISaveChangesService saveChangesService)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _saveChangesService = saveChangesService;
        }

        public ServiceResult CreateNovel(CreateNovelDto dto)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            Novel novel = new Novel()
            {
                Title = dto.NovelTitle,
                TitleNormalized = StringUtilityMethods.NormalizeTitle(dto.NovelTitle),
                NovelStatus = dto.NovelStatus,
                UserId = userId,
            };

            _db.Novels.Add(novel);

            // TODO: Incorporate the save changes service into new service and return boolean signaling whether save went through.
            return _saveChangesService.TrySave();
        }

        public EditNovelDto? GetEditNovelDto(int novelId)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Novel? novel = _db.Novels.Where(u => u.UserId == userId && u.Id == novelId).FirstOrDefault();

            if (novel == null)
            {
                return null;
            }

            return MapToEditNovelDto(novel);
        }

        public ViewNovelDto? GetViewNovelDto(string titleNormalized)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Novel? novel = _db.Novels.Where(u => u.UserId == userId && u.TitleNormalized == titleNormalized).FirstOrDefault();

            if (novel == null)
            {
                return null;
            }

            return MapToViewNovelDto(novel);
        }

        public ServiceResult EditNovel(EditNovelDto dto)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Novel? novel = _db.Novels.Where(u => u.UserId == userId && u.Id == dto.NovelId).FirstOrDefault();

            if (novel == null)
            {
                return ServiceResult.NotFound;
            }
            
            novel.Title = dto.NovelTitle;
            novel.TitleNormalized = StringUtilityMethods.NormalizeTitle(dto.NovelTitle);
            novel.NovelStatus = dto.NovelStatus;

            // TODO: Incorporate the save changes service into new service and return boolean signaling whether save went through.
            return _saveChangesService.TrySave();
        }

        public List<ViewNovelDto> GetAllViewNovelDto()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var novels = _db.Novels.Where(u => u.UserId == userId).Select(u => new ViewNovelDto
            {
                NovelTitle = u.Title,
                NovelStatus = u.NovelStatus,
                NovelId = u.Id,
            }).ToList();

            return novels;
        }

        public ServiceResult DeleteNovel(int novelId)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Novel? novel = _db.Novels.Where(u => u.UserId == userId && u.Id == novelId).FirstOrDefault();

            if(novel == null)
            {
                return ServiceResult.NotFound;
            }

            _db.Novels.Remove(novel);
            return _saveChangesService.TrySave();
        }

        public List<string> GetNovelTitleSuggestions(string title)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var suggestions = _db.Novels.Where(u => u.UserId == userId && u.TitleNormalized.Contains(StringUtilityMethods.NormalizeTitle(title)))
                .OrderBy(u => u.TitleNormalized).Select(n => n.Title).Take(5).ToList();

            return suggestions;
        }

        private EditNovelDto MapToEditNovelDto(Novel novel)
        {
            return new EditNovelDto()
            {
                NovelTitle = novel.Title,
                NovelStatus = novel.NovelStatus
            };
        }

        private ViewNovelDto MapToViewNovelDto(Novel novel)
        {
            return new ViewNovelDto()
            {
                NovelTitle = novel.Title,
                NovelStatus = novel.NovelStatus,
                NovelId = novel.Id
            };
        }
    }
}

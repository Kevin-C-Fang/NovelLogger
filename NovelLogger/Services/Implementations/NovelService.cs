using Microsoft.AspNetCore.Mvc;
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
    public class NovelService: INovelService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;

        public NovelService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork)
        {
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
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

            _unitOfWork.Novel.Add(novel);

            return _unitOfWork.TrySave();
        }

        public ViewNovelDto? GetViewNovelDto(string titleNormalized)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Novel? novel = _unitOfWork.Novel.GetFirstOrDefault(u => u.UserId == userId && u.TitleNormalized == titleNormalized);

            if (novel == null)
            {
                return null;
            }

            return MapToViewNovelDto(novel);
        }

        public EditNovelDto? GetEditNovelDto(int novelId)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Novel? novel = _unitOfWork.Novel.GetFirstOrDefault(u => u.UserId == userId && u.Id == novelId);

            if (novel == null)
            {
                return null;
            }

            return MapToEditNovelDto(novel);
        }

        public ServiceResult EditNovel(EditNovelDto dto)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Novel? novel = _unitOfWork.Novel.GetFirstOrDefault(u => u.UserId == userId && u.Id == dto.NovelId, null, true);

            if (novel == null)
            {
                return ServiceResult.NotFound;
            }
            
            novel.Title = dto.NovelTitle;
            novel.TitleNormalized = StringUtilityMethods.NormalizeTitle(dto.NovelTitle);
            novel.NovelStatus = dto.NovelStatus;

            return _unitOfWork.TrySave();
        }

        public List<ViewNovelDto> GetAllViewNovelDto()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var novels = _unitOfWork.Novel.GetAll(u => u.UserId == userId).Select(u => new ViewNovelDto
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
            Novel? novel = _unitOfWork.Novel.GetFirstOrDefault(u => u.UserId == userId && u.Id == novelId);

            if(novel == null)
            {
                return ServiceResult.NotFound;
            }

            _unitOfWork.Novel.Remove(novel);

            return _unitOfWork.TrySave();
        }

        public List<string> GetNovelTitleSuggestions(string title)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var suggestions = _unitOfWork.Novel.GetNovelTitleSuggestions(u => u.UserId == userId && u.TitleNormalized.Contains(StringUtilityMethods.NormalizeTitle(title)));

            return suggestions;
        }

        private EditNovelDto MapToEditNovelDto(Novel novel)
        {
            return new EditNovelDto()
            {
                NovelTitle = novel.Title,
                NovelStatus = novel.NovelStatus,
                NovelId = novel.Id,
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

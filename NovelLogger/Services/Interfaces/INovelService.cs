using NovelLogger.Models.DTOs;
using NovelLogger.Services.Implementations;

namespace NovelLogger.Services.Interfaces
{
    public interface INovelService
    {
        public ServiceResult CreateNovel(CreateNovelDto dto);
        EditNovelDto? GetEditNovelDto(int novelId);
        ViewNovelDto? GetViewNovelDto(string titleNormalized);
        public ServiceResult EditNovel(EditNovelDto dto);
        public List<ViewNovelDto> GetAllViewNovelDto();
        public ServiceResult DeleteNovel(int novelId);
        public List<string> GetNovelTitleSuggestions(string title);
    }
}

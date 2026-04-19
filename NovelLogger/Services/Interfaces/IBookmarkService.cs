using NovelLogger.Data.Repositories;
using NovelLogger.Models.DTOs;

namespace NovelLogger.Services.Interfaces
{
    public interface IBookmarkService
    {
        public ServiceResult CreateBookmark(CreateBookmarkDto dto);
        public ViewBookmarkDto? GetViewBookmarkDto(int bookmarkId);
        public EditBookmarkDto? GetEditBookmarkDto(int bookmarkId);
        public ServiceResult EditBookmark(EditBookmarkDto dto);
        public List<ViewBookmarkDto> GetAllViewBookmarkDto();
        public ServiceResult DeleteBookmark(int bookmarkId);
    }
}

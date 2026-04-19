using NovelLogger.Services.Implementations;

namespace NovelLogger.Data.Repositories.IRepositories
{
    public interface IUnitOfWork
    {
        INovelRepository Novel { get; }
        IBookmarkRepository Bookmark { get; }

        ServiceResult TrySave();
    }
}

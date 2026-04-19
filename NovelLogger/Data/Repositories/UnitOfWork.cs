using Microsoft.Build.Tasks.Deployment.Bootstrapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NovelLogger.Data.Repositories.IRepositories;
using NovelLogger.Services.Implementations;
using NovelLogger.Utility;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace NovelLogger.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        public INovelRepository Novel { get; private set; }
        public IBookmarkRepository Bookmark { get; private set; }
        private readonly ILogger<UnitOfWork> _logger;

        public UnitOfWork(ApplicationDbContext db, ILogger<UnitOfWork> logger)
        {
            _db = db;
            Novel = new NovelRepository(_db);
            Bookmark = new BookmarkRepository(_db);
            _logger = logger;
        }
        public ServiceResult TrySave()
        {
            try
            {
                _db.SaveChanges();
                return ServiceResult.Success;
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx &&
                (sqlEx.Number == 2601 || sqlEx.Number == 2627))
            {
                var msg = sqlEx.Message;

                if (msg.Contains(DbIndexStrings.NovelUniqueIndex))
                {
                    return ServiceResult.NovelTitleNormDuplicate;
                }

                if (msg.Contains(DbIndexStrings.BookmarkUniqueIndex))
                {
                    return ServiceResult.BookmarkUrlDuplicate;
                }

                _logger.LogWarning(ex, sqlEx.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, ex.Message);
                throw;
            }
        }
    }

    public enum ServiceResult
    {
        Success,
        NotFound,
        NovelTitleNormDuplicate,
        BookmarkUrlDuplicate,
    }
}

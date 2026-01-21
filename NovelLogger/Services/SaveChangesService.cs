using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NovelLogger.Data;
using NovelLogger.Utility;

namespace NovelLogger.Services
{
    public class SaveChangesService : ISaveChangesService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<SaveChangesService> _logger;

        public SaveChangesService(ApplicationDbContext db, ILogger<SaveChangesService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public SaveResult TrySave()
        {
            try
            {
                _db.SaveChanges();
                return SaveResult.Success;
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && 
                (sqlEx.Number == 2601 || sqlEx.Number == 2627))
            {
                var msg = sqlEx.Message;

                if (msg.Contains(DbIndexStrings.NovelUniqueIndex)) 
                { 
                    return SaveResult.NovelTitleNormDuplicate;
                }

                if (msg.Contains(DbIndexStrings.BookmarkUniqueIndex))
                {
                    return SaveResult.BookmarkUrlDuplicate;
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

    public enum SaveResult
    {
        Success,
        NovelTitleNormDuplicate,
        BookmarkUrlDuplicate,
    }
}

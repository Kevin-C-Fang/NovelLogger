using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NovelLogger.Data;

namespace NovelLogger.Services
{
    public class SaveChangesService : ISaveChangesService
    {
        private readonly ApplicationDbContext _db;
        public SaveChangesService(ApplicationDbContext db)
        {
            _db = db;
        }

        public SaveResult TrySave()
        {
            try
            {
                _db.SaveChanges();
                return SaveResult.Success;
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601)
            {
                return SaveResult.Duplicate;
            }
            catch
            {
                throw;
            }
        }
    }

    public enum SaveResult
    {
        Success,
        Duplicate,
    }
}

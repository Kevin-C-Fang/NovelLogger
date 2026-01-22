using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NovelLogger.Services;

namespace NovelLogger.Data.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<SaveChangesService> _logger;

        public DbInitializer(ApplicationDbContext db, ILogger<SaveChangesService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public void Initialize()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, ex.Message);
                throw;
            }

            return;
        }
    }
}

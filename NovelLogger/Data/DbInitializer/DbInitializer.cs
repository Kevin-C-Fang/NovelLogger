using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NovelLogger.Services.Implementations;

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
            var strategy = _db.Database.CreateExecutionStrategy();

            try
            {
                strategy.Execute(() =>
                {
                    _db.Database.Migrate();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database migration failed during startup.");
            }
        }
    }
}

using NovelLogger.Data.Repositories.IRepositories;
using NovelLogger.Models.Entities;
using NovelLogger.Utility;
using System.Linq.Expressions;
using System.Security.Claims;

namespace NovelLogger.Data.Repositories
{
    public class NovelRepository : Repository<Novel>, INovelRepository
    {
        private ApplicationDbContext _db;

        public NovelRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public List<string> GetNovelTitleSuggestions(Expression<Func<Novel, bool>> filter)
        {
            var suggestions = _db.Novels.Where(filter).OrderBy(u => u.TitleNormalized).Select(n => n.Title).Take(5).ToList();

            return suggestions;
        }
    }
}

using NovelLogger.Data.Repositories.IRepositories;
using NovelLogger.Models.Entities;

namespace NovelLogger.Data.Repositories
{
    public class BookmarkRepository : Repository<Bookmark>, IBookmarkRepository
    {
        private ApplicationDbContext _db;

        public BookmarkRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
    }
}

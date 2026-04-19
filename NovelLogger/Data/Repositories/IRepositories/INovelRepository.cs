using NovelLogger.Models.Entities;
using System.Linq.Expressions;

namespace NovelLogger.Data.Repositories.IRepositories
{
    public interface INovelRepository : IRepository<Novel>
    {
        List<string> GetNovelTitleSuggestions(Expression<Func<Novel, bool>> filter);
    }
}

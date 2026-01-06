using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NovelLogger.Models;

namespace NovelLogger.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<Novel> Novels { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Novel>(e =>
            {
                e.HasIndex(x => new { x.UserId, x.TitleNormalized }).IsUnique();
            });

            builder.Entity<Bookmark>(e =>
            {
                e.HasIndex(x => new { x.UserId, x.NovelId, x.DateAdded });
            });
        }
    }
}

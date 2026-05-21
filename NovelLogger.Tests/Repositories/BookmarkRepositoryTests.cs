using Microsoft.EntityFrameworkCore;
using NovelLogger.Data;
using NovelLogger.Data.Repositories;
using NovelLogger.Data.Repositories.IRepositories;
using NovelLogger.Models.Entities;
using NovelLogger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovelLogger.Tests.Repositories
{
    public class BookmarkRepositoryTests
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly BookmarkRepository _bookmarkRepository;
        private const string _userId = "test-user-id";

        public BookmarkRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _bookmarkRepository = new BookmarkRepository(_dbContext);
        }

        [Fact]
        public void Add_WhenEntityIsAdded_AddsEntityToDbSet()
        {
            var bookmark = new Bookmark
            {
                Id = 1,
                UserId = _userId,
                Url = "https://example.com",
                Notes = "notes",
                IsSaved = true,
                Novel = new Novel
                {
                    Id = 1,
                    Title = "Test Novel",
                    TitleNormalized = "test novel",
                    NovelStatus = NovelStatusStrings.Completed,
                    UserId = _userId
                }
            };

            _bookmarkRepository.Add(bookmark);
            _dbContext.SaveChanges();

            Assert.Single(_dbContext.Bookmarks);
            Assert.Equal("https://example.com", _dbContext.Bookmarks.First().Url);
        }

        [Fact]
        public void GetAll_WhenNoFilterIsProvided_ReturnsAllEntities()
        {
            SeedBookmarksAndNovel();

            var result = _bookmarkRepository.GetAll().ToList();

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void GetAll_WhenFilterIsProvided_ReturnsFilteredEntities()
        {
            SeedBookmarksAndNovel();

            var result = _bookmarkRepository.GetAll(n => n.IsSaved == true).ToList();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetAll_WhenIncludeIsProvided_ReturnsIncludedEntities()
        {
            SeedBookmarksAndNovel();

            var result = _bookmarkRepository.GetAll(includeProperties: "Novel").ToList();

            Assert.Equal(3, result.Count);
            Assert.All(result, n => Assert.NotNull(n.Novel));
        }

        [Fact]
        public void GetFirstOrDefault_WhenMatchingEntityExists_ReturnsFirstMatchingEntity()
        {
            SeedBookmarksAndNovel();

            var result = _bookmarkRepository.GetFirstOrDefault(n => n.Url == "https://example1.com");

            Assert.NotNull(result);
            Assert.Equal("https://example1.com", result.Url);
        }

        [Fact]
        public void GetFirstOrDefault_WhenNoMatchingEntityExists_ReturnsNull()
        {
            SeedBookmarksAndNovel();

            var result = _bookmarkRepository.GetFirstOrDefault(n => n.Id == 5);

            Assert.Null(result);
        }

        [Fact]
        public void GetFirstOrDefault_WhenIncludeIsProvided_ReturnsIncludedEntities()
        {
            SeedBookmarksAndNovel();

            var result = _bookmarkRepository.GetFirstOrDefault(n => n.Url == "https://example1.com", includeProperties: "Novel");

            Assert.NotNull(result);
            Assert.NotNull(result.Novel);
        }

        [Fact]
        public void GetFirstOrDefault_WhenTrackedIsFalse_ReturnsUntrackedEntity()
        {
            SeedBookmarksAndNovel();

            var result = _bookmarkRepository.GetFirstOrDefault(n => n.Id == 1, tracked: false);

            Assert.NotNull(result);
            Assert.Equal(EntityState.Detached, _dbContext.Entry(result).State);
        }

        [Fact]
        public void GetFirstOrDefault_WhenTrackedIsTrue_ReturnsTrackedEntity()
        {
            SeedBookmarksAndNovel();

            var result = _bookmarkRepository.GetFirstOrDefault(n => n.Id == 1, tracked: true);

            Assert.NotNull(result);
            Assert.Equal(EntityState.Unchanged, _dbContext.Entry(result).State);
        }

        [Fact]
        public void Update_WhenEntityIsUpdated_UpdatesEntityInDbSet()
        {
            SeedBookmarksAndNovel();

            var novel = _bookmarkRepository.GetFirstOrDefault(n => n.Id == 1, tracked: true);
            novel.Notes = "New notes";

            _bookmarkRepository.Update(novel);
            _dbContext.SaveChanges();

            var result = _bookmarkRepository.GetFirstOrDefault(n => n.Id == 1);
            Assert.Equal("New notes", result.Notes);
        }

        [Fact]
        public void Remove_WhenEntityIsRemoved_RemovesEntityFromDbSet()
        {
            SeedBookmarksAndNovel();

            var bookmark = _bookmarkRepository.GetFirstOrDefault(n => n.Id == 1, tracked: true);

            _bookmarkRepository.Remove(bookmark);
            _dbContext.SaveChanges();

            var result = _bookmarkRepository.GetAll().ToList();
            Assert.Equal(2, result.Count());
            Assert.DoesNotContain(result, n => n.Id == 1);
        }

        private void SeedBookmarksAndNovel()
        {
            _dbContext.Novels.Add(
                new Novel
                {
                    Id = 1,
                    Title = "Novel 1",
                    TitleNormalized = "novel 1",
                    NovelStatus = NovelStatusStrings.Completed,
                    UserId = _userId
                }
            );

            _dbContext.Bookmarks.AddRange(
                new Bookmark
                {
                    Id = 1,
                    UserId = _userId,
                    Url = "https://example1.com",
                    Notes = "notes 1",
                    IsSaved = true,
                    NovelId = 1
                },
                new Bookmark
                {
                    Id = 2,
                    UserId = _userId,
                    Url = "https://example2.com",
                    Notes = "notes 2",
                    IsSaved = true,
                    NovelId = 1
                },
                new Bookmark
                {
                    Id = 3,
                    UserId = _userId,
                    Url = "https://example3.com",
                    Notes = "notes 3",
                    IsSaved = false,
                    NovelId = 1
                }
            );

            _dbContext.SaveChanges();
        }
    }
}

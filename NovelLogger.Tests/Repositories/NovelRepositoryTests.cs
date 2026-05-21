using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using NovelLogger.Data;
using NovelLogger.Data.Repositories;
using NovelLogger.Data.Repositories.IRepositories;
using NovelLogger.Models.Entities;
using NovelLogger.Services.Implementations;
using NovelLogger.Services.Interfaces;
using NovelLogger.Utility;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NovelLogger.Tests.Repositories
{
    public class NovelRepositoryTests
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly NovelRepository _novelRepository;
        private const string _userId = "test-user-id";

        public NovelRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext =  new ApplicationDbContext(options);
            _novelRepository = new NovelRepository(_dbContext);
        }

        #region Base Repository Tests
        [Fact]
        public void Add_WhenEntityIsAdded_AddsEntityToDbSet()
        {
            var novel = new Novel
            {
                Id = 1,
                Title = "Chaotic Sword God",
                TitleNormalized = "chaotic sword god",
                NovelStatus = NovelStatusStrings.Completed,
                UserId = _userId
            };

            _novelRepository.Add(novel);
            _dbContext.SaveChanges();

            Assert.Single(_dbContext.Novels);
            Assert.Equal("Chaotic Sword God", _dbContext.Novels.First().Title);
        }

        [Fact]
        public void GetAll_WhenNoFilterIsProvided_ReturnsAllEntities()
        {
            SeedNovels();

            var result = _novelRepository.GetAll().ToList();

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void GetAll_WhenFilterIsProvided_ReturnsFilteredEntities()
        {
            SeedNovels();

            var result = _novelRepository.GetAll(n => n.NovelStatus == NovelStatusStrings.UpToDate).ToList();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetFirstOrDefault_WhenMatchingEntityExists_ReturnsFirstMatchingEntity()
        {
            SeedNovels();

            var result = _novelRepository.GetFirstOrDefault(n => n.Title == "Novel 1");

            Assert.NotNull(result);
            Assert.Equal("Novel 1", result.Title);
        }

        [Fact]
        public void GetFirstOrDefault_WhenNoMatchingEntityExists_ReturnsNull()
        {
            SeedNovels();

            var result = _novelRepository.GetFirstOrDefault(n => n.Id == 5);

            Assert.Null(result);
        }

        [Fact]
        public void GetFirstOrDefault_WhenTrackedIsFalse_ReturnsUntrackedEntity()
        {
            SeedNovels();

            var result = _novelRepository.GetFirstOrDefault(n => n.Id == 1, tracked: false);

            Assert.NotNull(result);
            Assert.Equal(EntityState.Detached, _dbContext.Entry(result).State);
        }

        [Fact]
        public void GetFirstOrDefault_WhenTrackedIsTrue_ReturnsTrackedEntity()
        {
            SeedNovels();

            var result = _novelRepository.GetFirstOrDefault(n => n.Id == 1, tracked: true);

            Assert.NotNull(result);
            Assert.Equal(EntityState.Unchanged, _dbContext.Entry(result).State);
        }

        [Fact]
        public void Update_WhenEntityIsUpdated_UpdatesEntityInDbSet()
        {
            SeedNovels();

            var novel = _novelRepository.GetFirstOrDefault(n => n.Id == 1, tracked: true);
            novel.Title = "New Novel";
            novel.TitleNormalized = "new novel";

            _novelRepository.Update(novel);
            _dbContext.SaveChanges();

            var result = _novelRepository.GetFirstOrDefault(n => n.Id == 1);
            Assert.Equal("New Novel", result.Title);
            Assert.Equal("new novel", result.TitleNormalized);
        }

        [Fact]
        public void Remove_WhenEntityIsRemoved_RemovesEntityFromDbSet()
        {
            SeedNovels();

            var novel = _novelRepository.GetFirstOrDefault(n => n.Id == 1, tracked: true);

            _novelRepository.Remove(novel);
            _dbContext.SaveChanges();

            var result = _novelRepository.GetAll().ToList();
            Assert.Equal(2, result.Count());
            Assert.DoesNotContain(result, n => n.Id == 1);
        }

        private void SeedNovels()
        {
            _dbContext.Novels.AddRange(
                new Novel
                {
                    Id = 1,
                    Title = "Novel 1",
                    TitleNormalized = "novel 1",
                    NovelStatus = NovelStatusStrings.UpToDate,
                    UserId = _userId
                },
                new Novel
                {
                    Id = 2,
                    Title = "Novel 2",
                    TitleNormalized = "novel 2",
                    NovelStatus = NovelStatusStrings.UpToDate,
                    UserId = _userId
                },
                new Novel
                {
                    Id = 3,
                    Title = "Novel 3",
                    TitleNormalized = "novel 3",
                    NovelStatus = NovelStatusStrings.Completed,
                    UserId = _userId
                }
            );

            _dbContext.SaveChanges();
        }
        #endregion

        #region Novel Title Suggestions
        [Fact]
        public void GetNovelTitleSuggestions_WhenMatchingExists_ReturnsMatchingTitles()
        {
            _dbContext.Novels.AddRange(
                new Novel { Title = "Novel 1", TitleNormalized = "novel 1", UserId = _userId, NovelStatus = "Completed"},
                new Novel { Title = "Novel 2", TitleNormalized = "novel 2", UserId = _userId, NovelStatus = "Completed" }
            );
            _dbContext.SaveChanges();

            var result = _novelRepository.GetNovelTitleSuggestions(u => u.UserId == _userId && u.TitleNormalized.StartsWith("novel"));

            Assert.Equal(
                new List<string> { "Novel 1", "Novel 2" },
                result
            );
        }

        [Fact]
        public void GetNovelTitleSuggestions_WhenMatchingIsNotOrdered_OrdersByTitleNormalized()
        {
            _dbContext.Novels.AddRange(
                new Novel { Title = "ABC Title", TitleNormalized = "abc title", UserId = _userId, NovelStatus = "Completed" },
                new Novel { Title = "AB Title", TitleNormalized = "ab title", UserId = _userId, NovelStatus = "Completed" }
            );
            _dbContext.SaveChanges();

            var result = _novelRepository.GetNovelTitleSuggestions(u => u.UserId == _userId && u.TitleNormalized.StartsWith("ab"));

            Assert.Equal(
                new List<string> { "AB Title", "ABC Title" },
                result
            );
        }

        [Fact]
        public void GetNovelTitleSuggestions_WhenGreaterThanFiveMatching_ReturnsMaximumOfFiveTitles()
        {
            _dbContext.Novels.AddRange(
                new Novel { Title = "AB Title", TitleNormalized = "ab title", UserId = _userId, NovelStatus = "Completed" },
                new Novel { Title = "ABC Title", TitleNormalized = "abc title", UserId = _userId, NovelStatus = "Completed" },
                new Novel { Title = "ABCD Title", TitleNormalized = "abcd title", UserId = _userId, NovelStatus = "Completed" },
                new Novel { Title = "ABCDE Title", TitleNormalized = "abcde title", UserId = _userId, NovelStatus = "Completed" },
                new Novel { Title = "ABCDEF Title", TitleNormalized = "abcdef title", UserId = _userId, NovelStatus = "Completed" },
                new Novel { Title = "ABCDEFG Title", TitleNormalized = "abcdefg title", UserId = _userId, NovelStatus = "Completed" }
            );
            _dbContext.SaveChanges();

            var result = _novelRepository.GetNovelTitleSuggestions(u => u.UserId == _userId && u.TitleNormalized.StartsWith("ab"));

            Assert.Equal(5, result.Count);
            Assert.Equal(
                new List<string> { "AB Title", "ABC Title", "ABCD Title", "ABCDE Title", "ABCDEF Title" },
                result
            );
            Assert.DoesNotContain("ABCDEFG Title", result);
        }

        [Fact]
        public void GetNovelTitleSuggestions_WhenNoNovelsMatchFilter_ReturnsEmptyList()
        {
            _dbContext.Novels.AddRange(
                new Novel { Title = "A Title", TitleNormalized = "a title", UserId = _userId, NovelStatus = "Completed" },
                new Novel { Title = "AB Title", TitleNormalized = "ab title", UserId = _userId, NovelStatus = "Completed" },
                new Novel { Title = "ABC Title", TitleNormalized = "abc title", UserId = _userId, NovelStatus = "Completed" }
            );
            _dbContext.SaveChanges();

            var result = _novelRepository.GetNovelTitleSuggestions(u => u.UserId == _userId && u.TitleNormalized.StartsWith("novel"));

            Assert.Empty(result);
        }
        #endregion
    }
}

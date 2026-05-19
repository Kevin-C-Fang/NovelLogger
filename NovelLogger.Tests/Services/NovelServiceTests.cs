using Microsoft.AspNetCore.Http;
using Moq;
using NovelLogger.Data.Repositories;
using NovelLogger.Data.Repositories.IRepositories;
using NovelLogger.Models.DTOs;
using NovelLogger.Models.Entities;
using NovelLogger.Services.Implementations;
using NovelLogger.Services.Interfaces;
using NovelLogger.Utility;
using System.Linq.Expressions;
using System.Security.Claims;

namespace NovelLogger.Tests.Services
{
    public class NovelServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly NovelService _service;
        private const string _userId = "test-user-id";

        public NovelServiceTests()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
{
                new Claim(ClaimTypes.NameIdentifier, _userId)
            }, "TestAuth"));

            var context = new DefaultHttpContext
            {
                User = user
            };

            var contextAccessor = new HttpContextAccessor
            {
                HttpContext = context
            };

            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _service = new NovelService(contextAccessor, _unitOfWorkMock.Object);
        }

        [Fact]
        public void CreateNovel_AddsNovel_ReturnsSuccessResult()
        {
            Novel? addedNovel = null;

            _unitOfWorkMock.Setup(u => u.Novel.Add(It.IsAny<Novel>())).Callback<Novel>(n => addedNovel = n);
            _unitOfWorkMock.Setup(u => u.TrySave()).Returns(ServiceResult.Success);

            var dto = new CreateNovelDto
            {
                NovelTitle = "Chaotic Sword God",
                NovelStatus = "Didn't Finish"
            };

            var result = _service.CreateNovel(dto);

            Assert.Equal(ServiceResult.Success, result);
            Assert.NotNull(addedNovel);
            Assert.Equal(dto.NovelTitle, addedNovel!.Title);
            Assert.Equal(StringUtilityMethods.NormalizeTitle(dto.NovelTitle), addedNovel.TitleNormalized);
            Assert.Equal(dto.NovelStatus, addedNovel.NovelStatus);
            Assert.Equal(_userId, addedNovel.UserId);

            _unitOfWorkMock.Verify(u => u.Novel.Add(It.IsAny<Novel>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.TrySave(), Times.Once);
        }

        [Fact]
        public void GetViewNovelDto_WhenNovelDoesNotExist_ReturnsNull()
        {
            _unitOfWorkMock
                .Setup(u => u.Novel.GetFirstOrDefault(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Novel, bool>>>(),
                    null,
                    false))
                .Returns((Novel?)null);

            var result = _service.GetViewNovelDto("missing-title");

            Assert.Null(result);
        }

        [Fact]
        public void GetViewNovelDto_WhenNovelExists_ReturnsDto()
        {
            var novel = new Novel
            {
                Id = 1,
                Title = "Chaotic Sword God",
                TitleNormalized = "chaotic sword god",
                NovelStatus = "Completed",
                UserId = _userId
            };

            _unitOfWorkMock
                .Setup(u => u.Novel.GetFirstOrDefault(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Novel, bool>>>(),
                    null,
                    false))
                .Returns(novel);

            var result = _service.GetViewNovelDto("Chaotic Sword God");

            Assert.NotNull(result);
            Assert.IsType<ViewNovelDto>(result);
            Assert.Equal(1, result.NovelId);
            Assert.Equal("Chaotic Sword God", result.NovelTitle);
            Assert.Equal("Completed", result.NovelStatus);
        }

        [Fact]
        public void GetEditNovelDto_WhenNovelDoesNotExist_ReturnsNull()
        {
            _unitOfWorkMock
                .Setup(u => u.Novel.GetFirstOrDefault(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Novel, bool>>>(),
                    null,
                    false))
                .Returns((Novel?)null);

            var result = _service.GetEditNovelDto(1);

            Assert.Null(result);
        }

        [Fact]
        public void GetEditNovelDto_WhenNovelExists_ReturnsDto()
        {
            var novel = new Novel
            {
                Id = 1,
                Title = "Chaotic Sword God",
                TitleNormalized = "chaotic sword god",
                NovelStatus = "Completed",
                UserId = _userId
            };

            _unitOfWorkMock
                .Setup(u => u.Novel.GetFirstOrDefault(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Novel, bool>>>(),
                    null,
                    false))
                .Returns(novel);

            var result = _service.GetEditNovelDto(1);

            Assert.NotNull(result);
            Assert.IsType<EditNovelDto>(result);
            Assert.Equal(1, result.NovelId);
            Assert.Equal("Chaotic Sword God", result.NovelTitle);
            Assert.Equal("Completed", result.NovelStatus);
        }

        [Fact]
        public void EditNovel_WhenNovelDoesNotExist_ReturnsNotFound()
        {
            _unitOfWorkMock
                .Setup(u => u.Novel.GetFirstOrDefault(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Novel, bool>>>(),
                    null,
                    true))
                .Returns((Novel?)null);

            var dto = new EditNovelDto
            {
                NovelId = 1,
                NovelTitle = "Chaotic Sword God",
                NovelStatus = "Up To Date"
            };

            var result = _service.EditNovel(dto);

            Assert.Equal(ServiceResult.NotFound, result);
        }

        [Fact]
        public void EditNovel_WhenNovelExists_UpdatesNovelAndReturnsSuccess()
        {
            var novel = new Novel
            {
                Id = 1,
                Title = "Chaotic Sword God",
                TitleNormalized = "chaotic sword god",
                NovelStatus = "Up To Date",
                UserId = _userId
            };

            _unitOfWorkMock
                .Setup(u => u.Novel.GetFirstOrDefault(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Novel, bool>>>(),
                    null,
                    true))
                .Returns(novel);

            _unitOfWorkMock
                .Setup(u => u.TrySave())
                .Returns(ServiceResult.Success);

            var dto = new EditNovelDto
            {
                NovelId = 1,
                NovelTitle = "Chaotic Sword God 2",
                NovelStatus = "Completed"
            };

            var result = _service.EditNovel(dto);

            Assert.Equal(ServiceResult.Success, result);
            Assert.Equal(dto.NovelTitle, novel.Title);
            Assert.Equal(StringUtilityMethods.NormalizeTitle(dto.NovelTitle), novel.TitleNormalized);
            Assert.Equal(dto.NovelStatus, novel.NovelStatus);

            _unitOfWorkMock.Verify(u => u.TrySave(), Times.Once);
        }

        [Fact]
        public void GetAllViewNovelDto_ReturnsOnlyCurrentUsersNovelsAsViewDtos()
        {
            var novels = new List<Novel>{
                    new Novel
                    {
                        Id = 1,
                        Title = "Chaotic Sword God",
                        NovelStatus = "Completed",
                        UserId = _userId
                    },
                    new Novel
                    {
                        Id = 2,
                        Title = "Chaotic Sword God 2",
                        NovelStatus = "Up to Date",
                        UserId = _userId
                    }
                };

            _unitOfWorkMock.Setup(u => u.Novel.GetAll(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Novel, bool>>>(), null))
                .Returns(novels);

            var result = _service.GetAllViewNovelDto();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            Assert.Equal(1, result[0].NovelId);
            Assert.Equal(novels[0].Title, result[0].NovelTitle);
            Assert.Equal(novels[0].NovelStatus, result[0].NovelStatus);

            Assert.Equal(2, result[1].NovelId);
            Assert.Equal(novels[1].Title, result[1].NovelTitle);
            Assert.Equal(novels[1].NovelStatus, result[1].NovelStatus);

            _unitOfWorkMock.Verify(u => u.Novel.GetAll(
                It.IsAny<System.Linq.Expressions.Expression<Func<Novel, bool>>>(), null),
                Times.Once);
        }

        [Fact]
        public void DeleteNovel_WhenNovelDoesNotExist_ReturnsNotFound()
        {
            _unitOfWorkMock
                .Setup(u => u.Novel.GetFirstOrDefault(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Novel, bool>>>(),
                    null,
                    false))
                .Returns((Novel?)null);

            var result = _service.DeleteNovel(1);

            Assert.Equal(ServiceResult.NotFound, result);
        }

        [Fact]
        public void DeleteNovel_WhenNovelExists_RemovesNovel_AndReturnsTrySaveResult()
        {
            var novel = new Novel
            {
                Id = 1,
                Title = "Chaotic Sword God",
                UserId = _userId
            };

            _unitOfWorkMock
                .Setup(u => u.Novel.GetFirstOrDefault(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Novel, bool>>>(),
                    null,
                    false))
                .Returns(novel);

            _unitOfWorkMock
                .Setup(u => u.TrySave())
                .Returns(ServiceResult.Success);

            var result = _service.DeleteNovel(1);

            Assert.Equal(ServiceResult.Success, result);
            _unitOfWorkMock.Verify(u => u.Novel.Remove(novel), Times.Once);
            _unitOfWorkMock.Verify(u => u.TrySave(), Times.Once);
        }

        [Fact]
        public void GetNovelTitleSuggestions_ValidTitle_ReturnsSuggestions()
        {
            var title = "cha";

            var expectedSuggestions = new List<string>
            {
                "Chaotic Sword God",
                "Chancellor Dammah",
            };

            _unitOfWorkMock
                .Setup(r => r.Novel.GetNovelTitleSuggestions(It.IsAny<Expression<Func<Novel, bool>>>()))
                .Returns(expectedSuggestions);

            var result = _service.GetNovelTitleSuggestions(title);

            Assert.Equal(expectedSuggestions, result);
            _unitOfWorkMock.Verify(u => u.Novel.GetNovelTitleSuggestions(It.IsAny<Expression<Func<Novel, bool>>>()),Times.Once);
        }

        [Fact]
        public void GetNovelTitleSuggestions_ValidTitle_PassesFilterWithUserIdAndNormalizedTitleStartsWith()
        {
            var userId = "test-user-id";
            var title = "Test";
            var normalizedTitle = StringUtilityMethods.NormalizeTitle(title);

            Expression<Func<Novel, bool>>? capturedFilter = null;

            _unitOfWorkMock.Setup(r => r.Novel.GetNovelTitleSuggestions(It.IsAny<Expression<Func<Novel, bool>>>()))
                           .Callback<Expression<Func<Novel, bool>>>(filter => capturedFilter = filter)
                           .Returns(new List<string>());

            var matchingNovel = new Novel
            {
                UserId = userId,
                Title = "Test 1",
                TitleNormalized = StringUtilityMethods.NormalizeTitle("Test 1")
            };

            var nonMatchingTitleNovel = new Novel
            {
                UserId = userId,
                Title = "Chaotic Sword God",
                TitleNormalized = "chaotic sword god"
            };

            _service.GetNovelTitleSuggestions(title);

            Assert.NotNull(capturedFilter);

            var compiledFilter = capturedFilter!.Compile();

            Assert.True(compiledFilter(matchingNovel));
            Assert.False(compiledFilter(nonMatchingTitleNovel));
        }
    }
}

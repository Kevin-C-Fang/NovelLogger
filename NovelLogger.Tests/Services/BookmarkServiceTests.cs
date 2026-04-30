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
    public class BookmarkServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<INovelService> _novelServiceMock;
        private readonly BookmarkService _bookmarkService;
        private const string _userId = "test-user-id";

        public BookmarkServiceTests()
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
            _novelServiceMock = new Mock<INovelService>();
            _bookmarkService = new BookmarkService(contextAccessor, _novelServiceMock.Object, _unitOfWorkMock.Object);
        }

        [Fact]
        public void CreateBookmark_WhenCreateNovelFails_ReturnsFailureAndDoesNotAddBookmark()
        {
            var dto = new CreateBookmarkDto()
            {
                NovelTitle = "Test Novel",
                TitleNormalized = "test novel",
                Url = "https://example.com",
                Notes = "notes",
                IsSaved = false,
                NovelStatus = NovelStatusStrings.UpToDate,
            };

            _novelServiceMock.Setup(s => s.GetViewNovelDto(dto.TitleNormalized))
                .Returns((ViewNovelDto?)null);

            _novelServiceMock.Setup(s => s.CreateNovel(It.IsAny<CreateNovelDto>()))
                .Returns(ServiceResult.Failed);

            var result = _bookmarkService.CreateBookmark(dto);

            Assert.True(result != ServiceResult.Success);
            _unitOfWorkMock.Verify(u => u.Bookmark.Add(It.IsAny<Bookmark>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.TrySave(), Times.Never);
        }

        [Fact]
        public void CreateBookmark_WhenEditNovelFails_ReturnsFailureAndDoesNotAddBookmark()
        {
            var dto = new CreateBookmarkDto()
            {
                NovelTitle = "Test Novel",
                TitleNormalized = "test-novel",
                Url = "https://example.com",
                Notes = "notes",
                IsSaved = false,
                NovelStatus = NovelStatusStrings.UpToDate,
            };

            var viewNovelDto = new ViewNovelDto
            {
                NovelId = 1,
                NovelTitle = "Test Novel",
                NovelStatus = NovelStatusStrings.UpToDate,
            };

            _novelServiceMock.Setup(s => s.GetViewNovelDto(dto.TitleNormalized))
                .Returns(viewNovelDto);

            _novelServiceMock.Setup(s => s.EditNovel(It.IsAny<EditNovelDto>()))
                .Returns(ServiceResult.Failed);

            var result = _bookmarkService.CreateBookmark(dto);

            Assert.True(result != ServiceResult.Success);
            _unitOfWorkMock.Verify(u => u.Bookmark.Add(It.IsAny<Bookmark>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.TrySave(), Times.Never);
        }

        [Fact]
        public void CreateBookmark_WhenNovelDoesNotExist_CreatesNovelThenAddsBookmark()
        {
            var dto = new CreateBookmarkDto
            {
                NovelTitle = "Test Novel",
                TitleNormalized = "test-novel",
                Url = "https://example.com",
                Notes = "notes",
                IsSaved = true,
                NovelStatus = NovelStatusStrings.UpToDate,
            };

            var novel = new Novel
            {
                Id = 1,
                UserId = _userId,
                Title = "Test Novel",
                TitleNormalized = "test novel"
            };

            _novelServiceMock.Setup(s => s.GetViewNovelDto(dto.TitleNormalized))
                .Returns((ViewNovelDto?)null);

            _novelServiceMock.Setup(s => s.CreateNovel(It.Is<CreateNovelDto>(n =>
                n.NovelTitle == dto.NovelTitle &&
                n.NovelStatus == dto.NovelStatus)))
                .Returns(ServiceResult.Success);

            _unitOfWorkMock.Setup(u => u.Bookmark.GetFirstOrDefault(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Bookmark, bool>>>(),
                    null,
                    false
                ))
                .Returns((Bookmark?)null);

            _unitOfWorkMock.Setup(u => u.Novel.GetFirstOrDefault(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Novel, bool>>>(),
                    null,
                    false
                ))
                .Returns(novel);

            _unitOfWorkMock.Setup(u => u.TrySave())
                .Returns(ServiceResult.Success);

            Bookmark? addedBookmark = null;

            _unitOfWorkMock.Setup(u => u.Bookmark.Add(It.IsAny<Bookmark>()))
                .Callback<Bookmark>(b => addedBookmark = b);

            var result = _bookmarkService.CreateBookmark(dto);

            Assert.Equal(ServiceResult.Success, result);
            Assert.NotNull(addedBookmark);

            Assert.Equal(_userId, addedBookmark.UserId);
            Assert.Equal(novel.Id, addedBookmark.NovelId);
            Assert.Equal(dto.Url, addedBookmark.Url);
            Assert.Equal(dto.Notes, addedBookmark.Notes);
            Assert.Equal(dto.IsSaved, addedBookmark.IsSaved);

            _novelServiceMock.Verify(s => s.CreateNovel(It.IsAny<CreateNovelDto>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.Bookmark.Add(It.IsAny<Bookmark>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.TrySave(), Times.Once);
        }

        [Fact]
        public void CreateBookmark_WhenNovelAlreadyExists_EditsNovelThenAddsBookmark()
        {
            var createBookmarkDto = new CreateBookmarkDto
            {
                NovelTitle = "Test Novel",
                TitleNormalized = "test novel",
                Url = "https://example.com",
                Notes = "notes",
                IsSaved = false,
                NovelStatus = NovelStatusStrings.Completed
            };

            var viewNovelDto = new ViewNovelDto
            {
                NovelId = 1,
                NovelTitle = "Test Novel",
                NovelStatus = NovelStatusStrings.UpToDate,
            };

            var novel = new Novel
            {
                Id = 1,
                UserId = _userId,
                Title = "Test Novel",
                TitleNormalized = "test novel",
                NovelStatus = NovelStatusStrings.Completed,
                
            };

            _novelServiceMock.Setup(s => s.GetViewNovelDto(createBookmarkDto.TitleNormalized))
                .Returns(viewNovelDto);

            _novelServiceMock.Setup(s => s.EditNovel(It.Is<EditNovelDto>(n =>
                n.NovelId == viewNovelDto.NovelId &&
                n.NovelTitle == createBookmarkDto.NovelTitle &&
                n.NovelStatus == createBookmarkDto.NovelStatus)))
                .Returns(ServiceResult.Success);

            _unitOfWorkMock.Setup(u => u.Bookmark.GetFirstOrDefault(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Bookmark, bool>>>(),
                    null,
                    false
                )).Returns((Bookmark?)null);

            _unitOfWorkMock.Setup(u => u.Novel.GetFirstOrDefault(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Novel, bool>>>(),
                    null,
                    false
                )).Returns(novel);

            _unitOfWorkMock.Setup(u => u.TrySave()).Returns(ServiceResult.Success);

            Bookmark? addedBookmark = null;

            _unitOfWorkMock.Setup(u => u.Bookmark.Add(It.IsAny<Bookmark>()))
                .Callback<Bookmark>(b => addedBookmark = b);

            var result = _bookmarkService.CreateBookmark(createBookmarkDto);

            Assert.Equal(ServiceResult.Success, result);
            Assert.NotNull(addedBookmark);

            Assert.Equal(_userId, addedBookmark.UserId);
            Assert.Equal(novel.Id, addedBookmark.NovelId);
            Assert.Equal(createBookmarkDto.Url, addedBookmark.Url);
            Assert.Equal(createBookmarkDto.Notes, addedBookmark.Notes);
            Assert.Equal(createBookmarkDto.IsSaved, addedBookmark.IsSaved);
            Assert.Equal(createBookmarkDto.NovelStatus, novel.NovelStatus);

            _novelServiceMock.Verify(s => s.EditNovel(It.IsAny<EditNovelDto>()), Times.Once);
            _novelServiceMock.Verify(s => s.CreateNovel(It.IsAny<CreateNovelDto>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.TrySave(), Times.Once);
        }

        [Fact]
        public void CreateBookmark_WhenLatestUnsavedBookmarkExists_RemovesItBeforeAddingNewBookmark()
        {
            var createBookmarkDto = new CreateBookmarkDto
            {
                NovelTitle = "Test Novel",
                TitleNormalized = "test novel",
                Url = "https://example.com",
                Notes = "notes",
                IsSaved = false,
                NovelStatus = NovelStatusStrings.UpToDate
            };

            var viewNovelDto = new ViewNovelDto
            {
                NovelId = 1,
                NovelTitle = "Test Novel",
                NovelStatus = NovelStatusStrings.UpToDate,
            };

            var novel = new Novel
            {
                Id = 1,
                UserId = _userId,
                Title = "Test Novel",
                TitleNormalized = "test-novel",
                NovelStatus = NovelStatusStrings.UpToDate,
            };

            var existingUnsavedBookmark = new Bookmark
            {
                Id = 1,
                UserId = _userId,
                Url = "https://google.com",
                Notes = "notes",
                IsSaved = false,
                Novel = novel
            };

            _novelServiceMock.Setup(s => s.GetViewNovelDto(createBookmarkDto.TitleNormalized))
                .Returns(viewNovelDto);

            _novelServiceMock.Setup(s => s.EditNovel(It.IsAny<EditNovelDto>()))
                .Returns(ServiceResult.Success);

            _unitOfWorkMock.SetupSequence(u => u.Bookmark.GetFirstOrDefault(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Bookmark, bool>>>(),
                    null,
                    false
                )).Returns(existingUnsavedBookmark);

            _unitOfWorkMock.Setup(u => u.Novel.GetFirstOrDefault(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Novel, bool>>>(),
                    null,
                    false
                )).Returns(novel);

            _unitOfWorkMock.Setup(u => u.TrySave()).Returns(ServiceResult.Success);

            Bookmark? addedBookmark = null;

            _unitOfWorkMock.Setup(u => u.Bookmark.Add(It.IsAny<Bookmark>()))
                .Callback<Bookmark>(b => addedBookmark = b);

            var result = _bookmarkService.CreateBookmark(createBookmarkDto);

            Assert.Equal(ServiceResult.Success, result);
            Assert.NotNull(addedBookmark);

            Assert.Equal(_userId, addedBookmark.UserId);
            Assert.Equal(novel.Id, addedBookmark.NovelId);
            Assert.Equal(createBookmarkDto.Url, addedBookmark.Url);
            Assert.Equal(createBookmarkDto.Notes, addedBookmark.Notes);
            Assert.Equal(createBookmarkDto.IsSaved, addedBookmark.IsSaved);
            Assert.Equal(createBookmarkDto.NovelStatus, novel.NovelStatus);

            _unitOfWorkMock.Verify(u => u.Bookmark.Remove(existingUnsavedBookmark), Times.Once);
            _unitOfWorkMock.Verify(u => u.Bookmark.Add(It.IsAny<Bookmark>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.TrySave(), Times.Once);
        }

        [Fact]
        public void GetViewBookmarkDto_WhenBookmarkDoesNotExist_ReturnsNull()
        {
            _unitOfWorkMock.Setup(u => u.Bookmark.GetFirstOrDefault(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Bookmark, bool>>>(),
                    "Novel",
                    false
                )).Returns((Bookmark?)null);

            var result = _bookmarkService.GetViewBookmarkDto(1);

            Assert.Null(result);
        }

        [Fact]
        public void GetViewBookmarkDto_WhenBookmarkExists_ReturnsDto()
        {
            var bookmark = new Bookmark
            {
                Id = 1,
                UserId = _userId,
                Url = "https://example.com",
                Notes = "notes",
                IsSaved = false,
                Novel = new Novel
                {
                    Id = 10,
                    Title = "Test Novel",
                    NovelStatus = NovelStatusStrings.UpToDate,
                }
            };

            _unitOfWorkMock.Setup(u => u.Bookmark.GetFirstOrDefault(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Bookmark, bool>>>(),
                    "Novel",
                    false
                )).Returns(bookmark);

            var result = _bookmarkService.GetViewBookmarkDto(1);

            Assert.NotNull(result);
            Assert.Equal(bookmark.Novel.Title, result.NovelTitle);
            Assert.Equal(bookmark.Id, result.BookmarkId);
            Assert.Equal(bookmark.Url, result.Url);
            Assert.Equal(bookmark.Notes, result.Notes);
            Assert.Equal(bookmark.IsSaved, result.IsSaved);
            Assert.Equal(bookmark.Novel.NovelStatus, result.NovelStatus);
        }

        [Fact]
        public void GetEditBookmarkDto_WhenBookmarkDoesNotExist_ReturnsNull()
        {
            _unitOfWorkMock.Setup(u => u.Bookmark.GetFirstOrDefault(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Bookmark, bool>>>(),
                    "Novel",
                    false
                )).Returns((Bookmark?)null);

            var result = _bookmarkService.GetEditBookmarkDto(1);

            Assert.Null(result);
        }

        [Fact]
        public void GetEditBookmarkDto_WhenBookmarkExists_ReturnsDto()
        {
            var bookmark = new Bookmark
            {
                Id = 1,
                UserId = _userId,
                Url = "https://example.com",
                Notes = "notes",
                IsSaved = false,
                Novel = new Novel
                {
                    Id = 10,
                    Title = "Test Novel",
                    NovelStatus = NovelStatusStrings.UpToDate,
                }
            };

            _unitOfWorkMock.Setup(u => u.Bookmark.GetFirstOrDefault(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Bookmark, bool>>>(),
                    "Novel",
                    false
                )).Returns(bookmark);

            var result = _bookmarkService.GetEditBookmarkDto(1);

            Assert.NotNull(result);
            Assert.Equal(bookmark.Novel.Title, result.NovelTitle);
            Assert.Equal(bookmark.Id, result.BookmarkId);
            Assert.Equal(bookmark.Url, result.Url);
            Assert.Equal(bookmark.Notes, result.Notes);
            Assert.Equal(bookmark.IsSaved, result.IsSaved);
            Assert.Equal(bookmark.Novel.NovelStatus, result.NovelStatus);
        }

        [Fact]
        public void EditBookmark_WhenBookmarkDoesNotExist_ReturnsNotFound()
        {
            var dto = new EditBookmarkDto
            {
                BookmarkId = 1,
                NovelTitle = "Test Novel",
                Url = "https://example.com",
                Notes = "notes",
                IsSaved = true,
                NovelStatus = NovelStatusStrings.Completed
            };

            _unitOfWorkMock.Setup(u => u.Bookmark.GetFirstOrDefault(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Bookmark, bool>>>(),
                    "Novel",
                    true
                )).Returns((Bookmark?)null);

            var result = _bookmarkService.EditBookmark(dto);

            Assert.Equal(ServiceResult.NotFound, result);
            _unitOfWorkMock.Verify(u => u.TrySave(), Times.Never);
        }

        [Fact]
        public void EditBookmark_WhenUrlDuplicateExists_ReturnsBookmarkUrlDuplicate()
        {
            var dto = new EditBookmarkDto
            {
                BookmarkId = 1,
                Url = "https://duplicate.com",
                Notes = "notes",
                IsSaved = true,
                NovelStatus = NovelStatusStrings.Completed
            };

            var existingBookmark = new Bookmark
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
                    NovelStatus = NovelStatusStrings.Completed
                }
            };

            var urlDuplicateBookmark = new Bookmark
            {
                Id = 2,
                UserId = _userId,
                Url = "https://duplicate.com",
                Notes = "duplicate notes",
                IsSaved = true,
                Novel = new Novel
                {
                    Id = 2,
                    Title = "Other Novel",
                    NovelStatus = NovelStatusStrings.Completed
                }
            };

            _unitOfWorkMock.SetupSequence(u => u.Bookmark.GetFirstOrDefault(
                    It.IsAny<Expression<Func<Bookmark, bool>>>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool>()
                ))
                .Returns(existingBookmark)
                .Returns(urlDuplicateBookmark);

            var result = _bookmarkService.EditBookmark(dto);

            Assert.Equal(ServiceResult.BookmarkUrlDuplicate, result);
            _unitOfWorkMock.Verify(u => u.TrySave(), Times.Never);
        }

        [Fact]
        public void EditBookmark_WhenBookmarkExists_UpdatesBookmarkAndNovelStatus()
        {
            var bookmark = new Bookmark
            {
                Id = 1,
                UserId = _userId,
                Url = "https://example.com",
                Notes = "notes",
                IsSaved = false,
                Novel = new Novel
                {
                    Id = 1,
                    Title = "Test Novel",
                    NovelStatus = NovelStatusStrings.UpToDate
                }
            };

            var dto = new EditBookmarkDto
            {
                BookmarkId = 1,
                Url = "https://example2.com",
                Notes = "New notes",
                IsSaved = true,
                NovelStatus = NovelStatusStrings.Completed
            };

            _unitOfWorkMock.SetupSequence(u => u.Bookmark.GetFirstOrDefault(
                    It.IsAny<Expression<Func<Bookmark, bool>>>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool>()
                ))
                .Returns(bookmark)
                .Returns((Bookmark?)null);

            _unitOfWorkMock.Setup(u => u.TrySave()).Returns(ServiceResult.Success);

            var result = _bookmarkService.EditBookmark(dto);

            Assert.Equal(ServiceResult.Success, result);
            Assert.Equal(dto.Url, bookmark.Url);
            Assert.Equal(dto.Notes, bookmark.Notes);
            Assert.Equal(dto.NovelStatus, bookmark.Novel.NovelStatus);
            Assert.Equal(dto.IsSaved, bookmark.IsSaved);
            _unitOfWorkMock.Verify(u => u.TrySave(), Times.Once);
        }

        [Fact]
        public void EditBookmark_WhenBookmarkAlreadySaved_DoesNotChangeIsSavedToFalse()
        {
            var bookmark = new Bookmark
            {
                Id = 1,
                UserId = _userId,
                Url = "https://example.com",
                Notes = "old notes",
                IsSaved = true,
                Novel = new Novel
                {
                    Id = 1,
                    Title = "Test Novel",
                    NovelStatus = NovelStatusStrings.UpToDate
                }
            };

            var dto = new EditBookmarkDto
            {
                BookmarkId = 1,
                Url = "https://example2.com",
                Notes = "new notes",
                IsSaved = false,
                NovelStatus = NovelStatusStrings.Completed
            };

            _unitOfWorkMock.SetupSequence(u => u.Bookmark.GetFirstOrDefault(
                    It.IsAny<Expression<Func<Bookmark, bool>>>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool>()
                ))
                .Returns(bookmark)
                .Returns((Bookmark?)null);

            _unitOfWorkMock.Setup(u => u.TrySave())
                .Returns(ServiceResult.Success);

            var result = _bookmarkService.EditBookmark(dto);

            Assert.Equal(ServiceResult.Success, result);
            Assert.True(bookmark.IsSaved);
            Assert.Equal(dto.Url, bookmark.Url);
            Assert.Equal(dto.Notes, bookmark.Notes);
            Assert.Equal(dto.NovelStatus, bookmark.Novel.NovelStatus);
            _unitOfWorkMock.Verify(u => u.TrySave(), Times.Once);
        }

        [Fact]
        public void GetAllViewBookmarkDto_ReturnsOnlyCurrentUsersBookmarksAsViewDtos()
        {
            var bookmarks = new List<Bookmark>{
                new Bookmark()
                {
                    Id = 1,
                    UserId = _userId,
                    Url = "https://example.com",
                    Notes = "notes",
                    IsSaved = false,
                    DateAdded = DateTime.UtcNow,
                    Novel = new Novel
                    {
                        Id = 1,
                        Title = "Test Novel",
                        NovelStatus = NovelStatusStrings.Completed,
                        UserId = _userId
                    }
                },
                new Bookmark()
                {
                    Id = 2,
                    UserId = _userId,
                    Url = "https://example2.com",
                    Notes = "notes 2",
                    IsSaved = true,
                    DateAdded = DateTime.UtcNow,
                    Novel = new Novel
                    {
                        Id = 2,
                        Title = "Test Novel 2",
                        NovelStatus = NovelStatusStrings.UpToDate,
                        UserId = _userId
                    }
                }
            };

            _unitOfWorkMock.Setup(u => u.Bookmark.GetAll(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Bookmark, bool>>>(),
                    "Novel"
                )).Returns(bookmarks);

            var result = _bookmarkService.GetAllViewBookmarkDto();

            Assert.NotNull(result);
            Assert.Equal(bookmarks.Count, result.Count);

            Assert.Equal(bookmarks[0].Id, result[0].BookmarkId);
            Assert.Equal(bookmarks[0].Url, result[0].Url);
            Assert.Equal(bookmarks[0].Notes, result[0].Notes);
            Assert.Equal(bookmarks[0].IsSaved, result[0].IsSaved);
            Assert.Equal(bookmarks[0].Novel.Title, result[0].NovelTitle);
            Assert.Equal(bookmarks[0].Novel.NovelStatus, result[0].NovelStatus);

            Assert.Equal(bookmarks[1].Id, result[1].BookmarkId);
            Assert.Equal(bookmarks[1].Url, result[1].Url);
            Assert.Equal(bookmarks[1].Notes, result[1].Notes);
            Assert.Equal(bookmarks[1].IsSaved, result[1].IsSaved);
            Assert.Equal(bookmarks[1].Novel.Title, result[1].NovelTitle);
            Assert.Equal(bookmarks[1].Novel.NovelStatus, result[1].NovelStatus);

            _unitOfWorkMock.Verify(u => u.Bookmark.GetAll(
                It.IsAny<System.Linq.Expressions.Expression<Func<Bookmark, bool>>>(), "Novel"), Times.Once);
        }

        [Fact]
        public void DeleteBookmark_WhenBookmarkDoesNotExist_ReturnsNotFound()
        {
            _unitOfWorkMock.Setup(u => u.Bookmark.GetFirstOrDefault(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Bookmark, bool>>>(),
                    null,
                    false
                )).Returns((Bookmark?)null);

            var result = _bookmarkService.DeleteBookmark(1);

            Assert.Equal(ServiceResult.NotFound, result);
            _unitOfWorkMock.Verify(u => u.Bookmark.Remove(It.IsAny<Bookmark>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.TrySave(), Times.Never);
        }

        [Fact]
        public void DeleteBookmark_WhenBookmarkExists_RemovesBookmarkAndSaves()
        {
            var bookmark = new Bookmark()
            {
                Id = 1,
                UserId = _userId,
                Url = "https://example.com",
                Notes = "notes",
                IsSaved = false,
                DateAdded = DateTime.UtcNow,
                Novel = new Novel
                {
                    Id = 1,
                    Title = "Test Novel",
                    NovelStatus = NovelStatusStrings.Completed,
                    UserId = _userId
                }
            };

            _unitOfWorkMock.Setup(u => u.Bookmark.GetFirstOrDefault(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Bookmark, bool>>>(),
                    null,
                    false
                )).Returns(bookmark);

            _unitOfWorkMock.Setup(u => u.TrySave()).Returns(ServiceResult.Success);

            var result = _bookmarkService.DeleteBookmark(1);

            Assert.Equal(ServiceResult.Success, result);
            _unitOfWorkMock.Verify(u => u.Bookmark.Remove(It.IsAny<Bookmark>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.TrySave(), Times.Once);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Moq;
using NovelLogger.Controllers;
using NovelLogger.Data.Repositories;
using NovelLogger.Models.DTOs;
using NovelLogger.Models.Entities;
using NovelLogger.Models.ViewModels;
using NovelLogger.Services.Implementations;
using NovelLogger.Services.Interfaces;
using NovelLogger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NovelLogger.Tests.Controllers
{
    public class BookmarkControllerTests
    {
        private readonly Mock<IBookmarkService> _bookmarkServiceMock;
        private readonly Mock<INovelService> _novelServiceMock;
        private readonly BookmarkController _controller;

        public BookmarkControllerTests()
        {
            _bookmarkServiceMock = new Mock<IBookmarkService>();
            _novelServiceMock = new Mock<INovelService>();
            _controller = new BookmarkController(_bookmarkServiceMock.Object, _novelServiceMock.Object);
        }

        [Fact]
        public void Index_Get_ReturnsIndexView()
        {
            var result = _controller.Index();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Create_Get_ReturnsCreateView()
        {
            var result = _controller.Create();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BookmarkVM>(viewResult.Model);

            Assert.Equal(NovelStatusStrings.StatusOptions, model.NovelStatusList);
        }

        [Fact]
        public void Create_Post_InvalidModelState_ReturnsViewWithSameVm()
        {
            BookmarkVM vm = new BookmarkVM()
            {
                NovelTitle = "Test Novel",
                Url = "https://www.google.com/",
                Notes = "Notes",
                IsSaved = false,
                NovelStatus = "Completed",
            };

            _controller.ModelState.AddModelError("NovelTitle", "Required");

            var result = _controller.Create(vm);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BookmarkVM>(viewResult.Model);

            Assert.Same(vm, model);
            Assert.Equal(NovelStatusStrings.StatusOptions, model.NovelStatusList);
        }

        [Fact]
        public void Create_Post_DuplicateTitle_ReturnsViewWithModelErrorAndSameVm()
        {
            BookmarkVM vm = new BookmarkVM()
            {
                NovelTitle = "Test Novel",
                Url = "https://www.google.com/",
                Notes = "Notes",
                IsSaved = false,
                NovelStatus = "Completed",
            };

            _bookmarkServiceMock.Setup(s => s.CreateBookmark(It.IsAny<CreateBookmarkDto>()))
                .Returns(ServiceResult.NovelTitleNormDuplicate);

            var result = _controller.Create(vm);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BookmarkVM>(viewResult.Model);

            Assert.Same(vm, model);
            Assert.Equal(NovelStatusStrings.StatusOptions, model.NovelStatusList);

            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey(nameof(vm.NovelTitle)));
            Assert.Equal(
                "A novel with this title already exists.",
                _controller.ModelState[nameof(vm.NovelTitle)]!.Errors[0].ErrorMessage);
        }

        [Fact]
        public void Create_Post_DuplicateUrl_ReturnsViewWithModelErrorAndSameVm()
        {
            BookmarkVM vm = new BookmarkVM()
            {
                NovelTitle = "Test Novel",
                Url = "https://www.google.com/",
                Notes = "Notes",
                IsSaved = false,
                NovelStatus = "Completed",
            };

            _bookmarkServiceMock.Setup(s => s.CreateBookmark(It.IsAny<CreateBookmarkDto>()))
                .Returns(ServiceResult.BookmarkUrlDuplicate);

            var result = _controller.Create(vm);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BookmarkVM>(viewResult.Model);

            Assert.Same(vm, model);
            Assert.Equal(NovelStatusStrings.StatusOptions, model.NovelStatusList);

            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey(nameof(vm.Url)));
            Assert.Equal(
                "A bookmark with this novel title and URL already exists.",
                _controller.ModelState[nameof(vm.Url)]!.Errors[0].ErrorMessage);
        }

        [Fact]
        public void Create_Post_ValidNovelAndSuccessfulCreation_ReturnsRedirectToIndex()
        {
            BookmarkVM vm = new BookmarkVM()
            {
                NovelTitle = "Test Novel",
                Url = "https://www.google.com/",
                Notes = "Notes",
                IsSaved = false,
                NovelStatus = "Completed",
            };

            _bookmarkServiceMock.Setup(s => s.CreateBookmark(It.IsAny<CreateBookmarkDto>())).Returns(ServiceResult.Success);

            var result = _controller.Create(vm);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(NovelController.Index), redirectResult.ActionName);
        }

        [Fact]
        public void ViewBookmark_Get_BookmarkNotFound_ReturnsNotFound()
        {
            int bookmarkId = 1;
            _bookmarkServiceMock
                .Setup(s => s.GetViewBookmarkDto(bookmarkId))
                .Returns((ViewBookmarkDto)null);

            var result = _controller.ViewBookmark(bookmarkId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void ViewBookmark_Get_BookmarkFound_ReturnsViewWithVM()
        {
            int bookmarkId = 1;
            var dto = new ViewBookmarkDto()
            {
                NovelTitle = "Test Novel",
                BookmarkId = 1,
                Url = "https://www.google.com/",
                Notes = "Notes",
                IsSaved = false,
                NovelStatus = "Completed",
            };

            _bookmarkServiceMock
                .Setup(s => s.GetViewBookmarkDto(bookmarkId))
                .Returns(dto);

            var result = _controller.ViewBookmark(bookmarkId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var vm = Assert.IsType<BookmarkVM>(viewResult.Model);

            Assert.Equal(dto.NovelTitle, vm.NovelTitle);
            Assert.Equal(dto.Url, vm.Url);
            Assert.Equal(dto.Notes, vm.Notes);
            Assert.Equal(dto.IsSaved, vm.IsSaved);
            Assert.Equal(dto.NovelStatus, vm.NovelStatus);
        }

        [Fact]
        public void Edit_Get_NovelNotFound_ReturnsNotFound()
        {
            int bookmarkId = 1;
            _bookmarkServiceMock
                .Setup(s => s.GetEditBookmarkDto(bookmarkId))
                .Returns((EditBookmarkDto)null);

            var result = _controller.Edit(bookmarkId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Edit_Get_BookmarkFound_ReturnsViewWithVm()
        {
            int bookmarkId = 1;
            var dto = new EditBookmarkDto
            {
                NovelTitle = "Test Novel",
                BookmarkId = 1,
                Url = "https://www.google.com/",
                Notes = "Notes",
                IsSaved = false,
                NovelStatus = "Completed",
            };

            _bookmarkServiceMock
                .Setup(s => s.GetEditBookmarkDto(bookmarkId))
                .Returns(dto);

            var result = _controller.Edit(bookmarkId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var vm = Assert.IsType<BookmarkVM>(viewResult.Model);

            Assert.Equal(dto.NovelTitle, vm.NovelTitle);
            Assert.Equal(dto.Url, vm.Url);
            Assert.Equal(dto.Notes, vm.Notes);
            Assert.Equal(dto.IsSaved, vm.IsSaved);
            Assert.Equal(dto.NovelStatus, vm.NovelStatus);
        }

        [Fact]
        public void Edit_Post_InvalidModelState_ReturnsViewWithSameVm()
        {
            BookmarkVM vm = new BookmarkVM()
            {
                NovelTitle = "Test Novel",
                Url = "https://www.google.com/",
                Notes = "Notes",
                IsSaved = false,
                NovelStatus = "Completed",
            };

            _controller.ModelState.AddModelError("NovelTitle", "Required");

            var result = _controller.Edit(vm);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BookmarkVM>(viewResult.Model);

            Assert.Same(vm, model);
            Assert.Equal(NovelStatusStrings.StatusOptions, model.NovelStatusList);
        }

        [Fact]
        public void Edit_Post_DuplicateUrl_ReturnsViewWithModelErrorAndSameVm()
        {
            BookmarkVM vm = new BookmarkVM()
            {
                NovelTitle = "Test Novel",
                Url = "https://www.google.com/",
                Notes = "Notes",
                IsSaved = false,
                NovelStatus = "Completed",
            };

            _bookmarkServiceMock.Setup(s => s.EditBookmark(It.IsAny<EditBookmarkDto>()))
                .Returns(ServiceResult.BookmarkUrlDuplicate);

            var result = _controller.Edit(vm);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BookmarkVM>(viewResult.Model);

            Assert.Same(vm, model);
            Assert.Equal(NovelStatusStrings.StatusOptions, model.NovelStatusList);

            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey(nameof(vm.Url)));
            Assert.Equal(
                "A bookmark with this novel and url already exists.",
                _controller.ModelState[nameof(vm.Url)]!.Errors[0].ErrorMessage);
        }

        [Fact]
        public void Edit_Post_ValidBookmarkAndSuccessfulEdit_ReturnsRedirectToIndex()
        {
            BookmarkVM vm = new BookmarkVM()
            {
                NovelTitle = "Test Novel",
                Url = "https://www.google.com/",
                Notes = "Notes",
                IsSaved = false,
                NovelStatus = "Completed",
            };

            _bookmarkServiceMock
                .Setup(s => s.EditBookmark(It.IsAny<EditBookmarkDto>()))
                .Returns(ServiceResult.Success);

            var result = _controller.Edit(vm);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(NovelController.Index), redirectResult.ActionName);
        }

        [Fact]
        public void GetAll_RetrieveAllUserBookmarks_ReturnsJsonResultWithAllBookmarks()
        {
            var dateAdded = new DateTime(2026, 1, 1, 0, 0, 0);
            var bookmarkDtos = new List<ViewBookmarkDto>
            {
                new ViewBookmarkDto
                {
                    BookmarkId = 1,
                    NovelTitle = "Test Novel",
                    Url = "https://Google.com",
                    DateAdded = dateAdded,
                    Notes = "Notes 1",
                    IsSaved = true
                },
                new ViewBookmarkDto
                {
                    BookmarkId = 2,
                    NovelTitle = "Test Novel 2",
                    Url = "https://Google.com",
                    DateAdded = dateAdded,
                    Notes = "Notes 2",
                    IsSaved = false
                }
            };

            _bookmarkServiceMock
                .Setup(s => s.GetAllViewBookmarkDto())
                .Returns(bookmarkDtos);

            var result = _controller.GetAll();

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var data = root.GetProperty("data");

            Assert.Equal(2, data.GetArrayLength());

            var first = data[0];
            var second = data[1];

            Assert.Equal("Test Novel",first.GetProperty("novel").GetProperty("title").GetString());
            Assert.Equal("https://Google.com",first.GetProperty("url").GetString());
            Assert.Equal("✓",first.GetProperty("hasNotes").GetString());
            Assert.Equal("✓",first.GetProperty("isSaved").GetString());
            Assert.Equal(1,first.GetProperty("bookmarkId").GetInt32());
            Assert.Equal(dateAdded,first.GetProperty("dateAdded").GetProperty("sort").GetDateTime());
            Assert.Equal(dateAdded.ToString("MM/dd/yyyy hh:mm:ss tt"),first.GetProperty("dateAdded").GetProperty("display").GetString());

            Assert.Equal("Test Novel 2",second.GetProperty("novel").GetProperty("title").GetString());
            Assert.Equal("https://Google.com",second.GetProperty("url").GetString());
            Assert.Equal("✓", second.GetProperty("hasNotes").GetString());
            Assert.Equal("✗", second.GetProperty("isSaved").GetString());
            Assert.Equal(2,second.GetProperty("bookmarkId").GetInt32());
            Assert.Equal(dateAdded,second.GetProperty("dateAdded").GetProperty("sort").GetDateTime());
            Assert.Equal(dateAdded.ToString("MM/dd/yyyy hh:mm:ss tt"),second.GetProperty("dateAdded").GetProperty("display").GetString());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void NovelTitleSuggestions_EmptyOrNullTitle_ReturnsJsonNull(string? title)
        {
            var result = _controller.NovelTitleSuggestions(title);

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Null(jsonResult.Value);
        }

        [Fact]
        public void NovelTitleSuggestions_ValidTitle_ReturnsSuggestions()
        {
            var title = "Test";
            var suggestions = new List<string> { "Test Novel 1", "Test Novel 2" };

            _novelServiceMock
                .Setup(s => s.GetNovelTitleSuggestions(title))
                .Returns(suggestions);

            var result = _controller.NovelTitleSuggestions(title);

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);

            var json = JsonSerializer.Serialize(jsonResult.Value);
            var value = JsonSerializer.Deserialize<List<string>>(json);

            Assert.NotNull(value);
            Assert.Equal(2, value.Count);
            Assert.Contains("Test Novel 1", value);
            Assert.Contains("Test Novel 2", value);
        }

        [Fact]
        public void Delete_BookmarkNotFound_ReturnsFailedJson()
        {
            int novelId = 1;

            _bookmarkServiceMock.Setup(s => s.DeleteBookmark(novelId)).Returns(ServiceResult.NotFound);

            var result = _controller.Delete(novelId);

            var jsonResult = Assert.IsType<JsonResult>(result);

            var value = jsonResult.Value;
            Assert.NotNull(value);

            var successProperty = value.GetType().GetProperty("success");
            Assert.NotNull(successProperty);
            Assert.False((bool)successProperty!.GetValue(value)!);
        }

        [Fact]
        public void Delete_BookmarkFound_ReturnsSuccessJson()
        {
            int novelId = 1;

            _bookmarkServiceMock.Setup(s => s.DeleteBookmark(novelId)).Returns(ServiceResult.Success);

            var result = _controller.Delete(novelId);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = jsonResult.Value;
            Assert.NotNull(value);

            var successProperty = value.GetType().GetProperty("success");
            Assert.NotNull(successProperty);
            Assert.True((bool)successProperty!.GetValue(value)!);
        }
    }
}

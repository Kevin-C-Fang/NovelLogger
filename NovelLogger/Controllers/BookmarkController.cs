using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovelLogger.Data;
using NovelLogger.Data.Repositories;
using NovelLogger.Models.DTOs;
using NovelLogger.Models.Entities;
using NovelLogger.Models.ViewModels;
using NovelLogger.Services.Implementations;
using NovelLogger.Services.Interfaces;
using NovelLogger.Utility;
using System.Security.Claims;

namespace NovelLogger.Controllers
{
    [Authorize]
    public class BookmarkController : Controller
    {
        private readonly IBookmarkService _bookmarkService;
        private readonly INovelService _novelService;

        public BookmarkController(IBookmarkService bookmarkService, INovelService novelService)
        {
            _bookmarkService = bookmarkService;
            _novelService = novelService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            BookmarkVM vm = new BookmarkVM()
            {
                NovelStatusList = NovelStatusStrings.StatusOptions,
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BookmarkVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.NovelStatusList = NovelStatusStrings.StatusOptions;
                return View(vm);
            }

            var createBookmarkDto = new CreateBookmarkDto()
            {
                NovelTitle = vm.NovelTitle,
                TitleNormalized = StringUtilityMethods.NormalizeTitle(vm.NovelTitle),
                NovelStatus = vm.NovelStatus,
                Url = vm.Url,
                Notes = vm.Notes,
                IsSaved = vm.IsSaved,
            };

            ServiceResult result = _bookmarkService.CreateBookmark(createBookmarkDto);

            if (result == ServiceResult.NovelTitleNormDuplicate)
            {
                ModelState.AddModelError(nameof(vm.NovelTitle), "A novel with this title already exists.");
                vm.NovelStatusList = NovelStatusStrings.StatusOptions;
                return View(vm);
            }
            else if(result == ServiceResult.BookmarkUrlDuplicate)
            {
                ModelState.AddModelError(nameof(vm.Url), "A bookmark with this novel title and url already exists.");
                vm.NovelStatusList = NovelStatusStrings.StatusOptions;
                return View(vm);
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ViewBookmark(int bookmarkId)
        {
            var viewBookmarkDto = _bookmarkService.GetViewBookmarkDto(bookmarkId);

            if (viewBookmarkDto == null) {
                return NotFound();
            }

            BookmarkVM vm = new BookmarkVM()
            {
                NovelTitle = viewBookmarkDto.NovelTitle,
                Url = viewBookmarkDto.Url,
                Notes = viewBookmarkDto.Notes,
                IsSaved = viewBookmarkDto.IsSaved,
                NovelStatus = viewBookmarkDto.NovelStatus,
            };

            return View(vm);
        }

        public IActionResult Edit(int bookmarkId)
        {
            var editBookmarkDto = _bookmarkService.GetEditBookmarkDto(bookmarkId);

            if (editBookmarkDto == null)
            {
                return NotFound();
            }

            BookmarkVM vm = new BookmarkVM()
            {
                NovelTitle = editBookmarkDto.NovelTitle,
                BookmarkId = editBookmarkDto.BookmarkId,
                Url = editBookmarkDto.Url,
                Notes = editBookmarkDto.Notes,
                IsSaved = editBookmarkDto.IsSaved,
                NovelStatus = editBookmarkDto.NovelStatus,
                NovelStatusList = NovelStatusStrings.StatusOptions,
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BookmarkVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.NovelStatusList = NovelStatusStrings.StatusOptions;

                return View(vm);
            }

            var editBookmarkDto = new EditBookmarkDto()
            {
                BookmarkId = vm.BookmarkId,
                NovelStatus = vm.NovelStatus,
                Url = vm.Url,
                Notes = vm.Notes,
                IsSaved = vm.IsSaved,
            };

            ServiceResult result = _bookmarkService.EditBookmark(editBookmarkDto);

            if (result == ServiceResult.BookmarkUrlDuplicate)
            {
                ModelState.AddModelError(nameof(vm.Url), "A bookmark with this novel and url already exists.");
                vm.NovelStatusList = NovelStatusStrings.StatusOptions;
                return View(vm);
            }

            return RedirectToAction(nameof(Index));
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var bookmarkDtos = _bookmarkService.GetAllViewBookmarkDto();
            var bookmarks = bookmarkDtos.Select(u => new
            {
                novel = new { title = u.NovelTitle },
                url = u.Url,
                dateAdded = new
                {
                    display = u.DateAdded.ToString("MM/dd/yyyy hh:mm:ss tt"),
                    sort = u.DateAdded
                },
                hasNotes = !string.IsNullOrEmpty(u.Notes) ? "✓" : "✗",
                isSaved = u.IsSaved ? "✓" : "✗",
                bookmarkId = u.BookmarkId,
            }).ToList();

            return Json(new { data = bookmarks });
        }

        [HttpGet]
        public IActionResult NovelTitleSuggestions(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return Json(null);
            }

            var suggestions = _novelService.GetNovelTitleSuggestions(title);
            return Json(suggestions);
        }

        [HttpDelete]
        public IActionResult Delete(int bookmarkId)
        {
            ServiceResult result = _bookmarkService.DeleteBookmark(bookmarkId);

            if (result == ServiceResult.NotFound)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}

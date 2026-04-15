using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovelLogger.Data;
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
    public class NovelController : Controller
    {
        private readonly INovelService _novelService;

        public NovelController(INovelService novelService)
        {
            _novelService = novelService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            NovelVM vm = new NovelVM()
            {
                NovelStatusList = NovelStatusStrings.StatusOptions,
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(NovelVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.NovelStatusList = NovelStatusStrings.StatusOptions;
                return View(vm);
            }

            var createNovelDto = new CreateNovelDto
            {
                NovelTitle = vm.NovelTitle,
                NovelStatus = vm.NovelStatus
            };

            var result = _novelService.CreateNovel(createNovelDto);

            if (result == ServiceResult.NovelTitleNormDuplicate)
            {
                ModelState.AddModelError(nameof(vm.NovelTitle), "A novel with this title already exists.");
                vm.NovelStatusList = NovelStatusStrings.StatusOptions;
                return View(vm);
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int novelId)
        {
            var editNovelDto = _novelService.GetEditNovelDto(novelId);

            if (editNovelDto == null)
            {
                return NotFound();
            }

            NovelVM vm = new NovelVM()
            {
                NovelTitle = editNovelDto.NovelTitle,
                NovelStatus = editNovelDto.NovelStatus,
                NovelStatusList = NovelStatusStrings.StatusOptions,
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(NovelVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.NovelStatusList = NovelStatusStrings.StatusOptions;
                return View(vm);
            }

            var editNovelDto = new EditNovelDto
            {
                NovelTitle = vm.NovelTitle,
                NovelStatus = vm.NovelStatus,
                NovelId = vm.NovelId,
            };

            var result = _novelService.EditNovel(editNovelDto);

            if (result == ServiceResult.NotFound)
            {
                return NotFound();
            }
            else if (result == ServiceResult.NovelTitleNormDuplicate)
            {
                ModelState.AddModelError(nameof(vm.NovelTitle), "This novel already exists.");
                vm.NovelStatusList = NovelStatusStrings.StatusOptions;
                return View(vm);
            }

            return RedirectToAction(nameof(Index));
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var novels = _novelService.GetAllViewNovelDto();
            return Json(new { data = novels });
        }

        [HttpDelete]
        public IActionResult Delete(int novelId)
        {
            var result = _novelService.DeleteNovel(novelId);

            if (result == ServiceResult.NotFound)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}

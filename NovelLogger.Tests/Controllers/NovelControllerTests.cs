using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NovelLogger.Controllers;
using NovelLogger.Data.Repositories;
using NovelLogger.Models.DTOs;
using NovelLogger.Models.ViewModels;
using NovelLogger.Services.Interfaces;
using NovelLogger.Utility;
using NuGet.ContentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovelLogger.Tests.Controllers
{
    public class NovelControllerTests
    {
        private readonly Mock<INovelService> _novelServiceMock;
        private readonly NovelController _controller;

        public NovelControllerTests()
        {
            _novelServiceMock = new Mock<INovelService>();
            _controller = new NovelController(_novelServiceMock.Object);
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
            var model = Assert.IsType<NovelVM>(viewResult.Model);

            Assert.Equal(NovelStatusStrings.StatusOptions, model.NovelStatusList);
        }

        [Fact]
        public void Create_Post_InvalidModelState_ReturnsViewWithSameVm()
        {
            var vm = new NovelVM()
            {
                NovelTitle = "Test Novel",
                NovelStatus = "Completed"
            };

            _controller.ModelState.AddModelError("NovelTitle", "Required");

            var result = _controller.Create(vm);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<NovelVM>(viewResult.Model);

            Assert.Same(vm, model);
            Assert.Equal(NovelStatusStrings.StatusOptions, model.NovelStatusList);
        }

        [Fact] 
        public void Create_Post_DuplicateTitle_ReturnsViewWithModelErrorAndSameVm()
        {
            var vm = new NovelVM
            {
                NovelTitle = "Test Novel",
                NovelStatus = "Completed"
            };

            _novelServiceMock.Setup(s => s.CreateNovel(It.IsAny<CreateNovelDto>()))
                .Returns(ServiceResult.NovelTitleNormDuplicate);

            var result = _controller.Create(vm);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<NovelVM>(viewResult.Model);

            Assert.Same(vm, model);
            Assert.Equal(NovelStatusStrings.StatusOptions, model.NovelStatusList);

            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey(nameof(vm.NovelTitle)));
            Assert.Equal(
                "A novel with this title already exists.",
                _controller.ModelState[nameof(vm.NovelTitle)]!.Errors[0].ErrorMessage);
        }

        [Fact]
        public void Create_Post_ValidNovelAndSuccessfulCreation_ReturnsRedirectToIndex()
        {
            var vm = new NovelVM
            {
                NovelTitle = "Test Novel",
                NovelStatus = "Completed"
            };

            _novelServiceMock
                .Setup(s => s.CreateNovel(It.IsAny<CreateNovelDto>()))
                .Returns(ServiceResult.Success);

            var result = _controller.Create(vm);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(NovelController.Index), redirectResult.ActionName);
        }

        [Fact]
        public void Edit_Get_NovelNotFound_ReturnsNotFound()
        {
            int novelId = 1;
            _novelServiceMock
                .Setup(s => s.GetEditNovelDto(novelId))
                .Returns((EditNovelDto)null);

            var result = _controller.Edit(novelId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Edit_Get_NovelFound_ReturnsViewWithVm()
        {
            int novelId = 1;
            var dto = new EditNovelDto
            {
                NovelTitle = "Test Novel",
                NovelStatus = "Completed"
            };

            _novelServiceMock
                .Setup(s => s.GetEditNovelDto(novelId))
                .Returns(dto);

            var result = _controller.Edit(novelId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var vm = Assert.IsType<NovelVM>(viewResult.Model);

            Assert.Equal(dto.NovelTitle, vm.NovelTitle);
            Assert.Equal(dto.NovelStatus, vm.NovelStatus);
            Assert.Equal(NovelStatusStrings.StatusOptions, vm.NovelStatusList);
        }

        [Fact]
        public void Edit_Post_InvalidModelState_ReturnsViewWithSameVm()
        {
            var vm = new NovelVM()
            {
                NovelTitle = "Test Novel",
                NovelStatus = "Completed"
            };

            _controller.ModelState.AddModelError("NovelTitle", "Required");

            var result = _controller.Edit(vm);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<NovelVM>(viewResult.Model);

            Assert.Same(vm, model);
            Assert.Equal(NovelStatusStrings.StatusOptions, model.NovelStatusList);
        }

        [Fact]
        public void Edit_Post_NovelNotFound_ReturnsNotFound()
        {
            var vm = new NovelVM
            {
                NovelTitle = "Test Novel",
                NovelStatus = "Completed"
            };

            _novelServiceMock.Setup(s => s.EditNovel(It.IsAny<EditNovelDto>()))
                .Returns(ServiceResult.NotFound);

            var result = _controller.Edit(vm);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Edit_Post_DuplicateTitle_ReturnsViewWithModelErrorAndSameVm()
        {
            var vm = new NovelVM
            {
                NovelTitle = "Test Novel",
                NovelStatus = "Completed"
            };

            _novelServiceMock.Setup(s => s.EditNovel(It.IsAny<EditNovelDto>()))
                .Returns(ServiceResult.NovelTitleNormDuplicate);

            var result = _controller.Edit(vm);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<NovelVM>(viewResult.Model);

            Assert.Same(vm, model);
            Assert.Equal(NovelStatusStrings.StatusOptions, model.NovelStatusList);

            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey(nameof(vm.NovelTitle)));
            Assert.Equal(
                "A novel with this title already exists.",
                _controller.ModelState[nameof(vm.NovelTitle)]!.Errors[0].ErrorMessage);
        }

        [Fact]
        public void Edit_Post_ValidNovelAndSuccessfulCreation_ReturnsRedirectToIndex()
        {
            var vm = new NovelVM
            {
                NovelTitle = "Test Novel",
                NovelStatus = "Completed"
            };

            _novelServiceMock
                .Setup(s => s.EditNovel(It.IsAny<EditNovelDto>()))
                .Returns(ServiceResult.Success);

            var result = _controller.Edit(vm);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(NovelController.Index), redirectResult.ActionName);
        }

        [Fact]
        public void GetAll_RetrieveAllUserNovels_ReturnsJsonResultWithAllNovels()
        {
            var novels = new List<ViewNovelDto>
            {
                new ViewNovelDto { NovelId = 1, NovelTitle = "Test Novel 1", NovelStatus = "Didn't Finish" },
                new ViewNovelDto { NovelId = 2, NovelTitle = "Test Novel 2", NovelStatus = "Completed" }
            };

            _novelServiceMock.Setup(s => s.GetAllViewNovelDto()).Returns(novels);

            var result = _controller.GetAll();

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = jsonResult.Value;
            Assert.NotNull(value);

            var dataProperty = value.GetType().GetProperty("data");
            Assert.NotNull(dataProperty);

            var jsonNovels = dataProperty!.GetValue(value);
            Assert.Same(novels, jsonNovels);
        }

        [Fact]
        public void Delete_NovelNotFound_ReturnsFailedJson()
        {
            int novelId = 1;

            _novelServiceMock.Setup(s => s.DeleteNovel(novelId)).Returns(ServiceResult.NotFound);

            var result = _controller.Delete(novelId);

            var jsonResult = Assert.IsType<JsonResult>(result);

            var value = jsonResult.Value;
            Assert.NotNull(value);

            var successProperty = value.GetType().GetProperty("success");
            Assert.NotNull(successProperty);
            Assert.False((bool)successProperty!.GetValue(value)!);
        }

        [Fact]
        public void Delete_NovelFound_ReturnsSuccessJson()
        {
            int novelId = 1;

            _novelServiceMock.Setup(s => s.DeleteNovel(novelId)).Returns(ServiceResult.Success);

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

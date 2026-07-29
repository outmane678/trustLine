using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

using AnonymousComplaintsAPI.Controllers;
using AnonymousComplaintsAPI.Services.Interfaces;
using AnonymousComplaintsAPI.DTOs.Requests;
using AnonymousComplaintsAPI.DTOs.Responses;

namespace TrustLine.Tests.Controllers
{
    public class CategoriesControllerTests
    {
        private readonly Mock<ICategoryService> _serviceMock = new();

        private CategoriesController CreateController() =>
            new CategoriesController(_serviceMock.Object);

        // =====================================================
        // GET ALL (paginated)
        // =====================================================
        [Fact]
        public async Task GetCategories_ShouldReturnOk()
        {
            var paginated = new PaginatedResponse<CategoryResponse>
            {
                Total = 1,
                Data = new List<CategoryResponse> { new CategoryResponse { Id = 1, Name = "Cat1" } },
                Page = 1,
                PerPage = 10
            };

            _serviceMock.Setup(x => x.GetCategoriesPaginatedAsync(It.IsAny<PaginationRequest>()))
                .ReturnsAsync(paginated);

            var result = await CreateController().GetCategories(new PaginationRequest());

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(ok.Value);
        }

        // =====================================================
        // GET BY ID
        // =====================================================
        [Fact]
        public async Task GetCategory_ShouldReturnOk()
        {
            _serviceMock.Setup(x => x.GetCategoryAsync(1))
                .ReturnsAsync(new CategoryResponse { Id = 1, Name = "Cat1" });

            var result = await CreateController().GetCategory(1);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task GetCategory_NotFound_ShouldReturnNotFound()
        {
            _serviceMock.Setup(x => x.GetCategoryAsync(99))
                .ReturnsAsync((CategoryResponse?)null);

            var result = await CreateController().GetCategory(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        // =====================================================
        // CREATE
        // =====================================================
        [Fact]
        public async Task PostCategory_ShouldReturnCreated()
        {
            var dto = new CategoryResponse { Name = "NewCat" };
            var created = new CategoryResponse { Id = 1, Name = "NewCat" };

            _serviceMock.Setup(x => x.CreateCategoryAsync(dto))
                .ReturnsAsync(created);

            var result = await CreateController().PostCategory(dto);

            Assert.IsType<CreatedAtActionResult>(result.Result);
        }

        // =====================================================
        // UPDATE
        // =====================================================
        [Fact]
        public async Task PutCategory_ShouldReturnOk()
        {
            var dto = new UpdateCategoryRequest { Name = "Updated" };

            _serviceMock.Setup(x => x.UpdateCategoryAsync(1, dto))
                .ReturnsAsync(new CategoryResponse { Id = 1, Name = "Updated" });

            var result = await CreateController().PutCategory(1, dto);

            Assert.IsType<OkResult>(result);
        }

        // =====================================================
        // SOFT DELETE
        // =====================================================
        [Fact]
        public async Task SoftDeleteCategory_ShouldReturnNoContent()
        {
            _serviceMock.Setup(x => x.ArchiveCategoryAsync(1))
                .Returns(Task.CompletedTask);

            var result = await CreateController().SoftDeleteCategory(1);

            Assert.IsType<NoContentResult>(result);
        }

        // =====================================================
        // RESTORE
        // =====================================================
        [Fact]
        public async Task RestoreCategory_ShouldReturnNoContent()
        {
            _serviceMock.Setup(x => x.RestoreCategoryAsync(1))
                .Returns(Task.CompletedTask);

            var result = await CreateController().RestoreCategory(1);

            Assert.IsType<NoContentResult>(result);
        }

        // =====================================================
        // HARD DELETE
        // =====================================================
        [Fact]
        public async Task HardDeleteCategory_ShouldReturnNoContent()
        {
            _serviceMock.Setup(x => x.DeleteCategoryAsync(1))
                .Returns(Task.CompletedTask);

            var result = await CreateController().HardDeleteCategory(1);

            Assert.IsType<NoContentResult>(result);
        }
    }
}

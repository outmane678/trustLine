using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using AnonymousComplaintsAPI.Controllers;
using AnonymousComplaintsAPI.Data;
using AnonymousComplaintsAPI.Models.Entities;
using AnonymousComplaintsAPI.Repositories.Interfaces;
using AnonymousComplaintsAPI.DTOs.Requests;
using AnonymousComplaintsAPI.DTOs.Responses;

namespace TrustLine.Tests.Controllers
{
    public class CategoriesControllerTests
    {
        private AnonymousComplaintsV002Context CreateDb()
        {
            var options = new DbContextOptionsBuilder<AnonymousComplaintsV002Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AnonymousComplaintsV002Context(options);
        }

        private CategoriesController CreateController(
            AnonymousComplaintsV002Context context,
            Mock<ICategoryRepository> repoMock)
        {
            return new CategoriesController(context, repoMock.Object);
        }

        // =========================
        // GET ALL
        // =========================
        [Fact]
        public async Task GetCategories_ShouldReturnList()
        {
            var context = CreateDb();

            var repoMock = new Mock<ICategoryRepository>();
            repoMock.Setup(x => x.GetAllWithTypeAsync())
                .ReturnsAsync(new List<Category>
                {
                    new Category { CategoryId = 1, Name = "Cat1" }
                });

            var controller = CreateController(context, repoMock);

            var result = await controller.GetCategories();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsAssignableFrom<IEnumerable<CategoryResponse>>(okResult.Value);

            Assert.Single(data);
        }

        // =========================
        // GET BY ID
        // =========================
        [Fact]
        public async Task GetCategory_ShouldReturnCategory()
        {
            var context = CreateDb();

            context.Categories.Add(new Category
            {
                CategoryId = 1,
                Name = "Test"
            });
            await context.SaveChangesAsync();

            var repoMock = new Mock<ICategoryRepository>();
            var controller = CreateController(context, repoMock);

            var result = await controller.GetCategory(1);

            Assert.NotNull(result.Value);
            Assert.Equal("Test", result.Value.Name);
        }

        [Fact]
        public async Task GetCategory_ShouldReturnNotFound()
        {
            var context = CreateDb();
            var repoMock = new Mock<ICategoryRepository>();
            var controller = CreateController(context, repoMock);

            var result = await controller.GetCategory(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        // =========================
        // POST
        // =========================
        [Fact]
        public async Task PostCategory_ShouldCreateCategory()
        {
            var context = CreateDb();
            var repoMock = new Mock<ICategoryRepository>();

            var controller = CreateController(context, repoMock);

            var dto = new CategoryResponse
            {
                Name = "NewCat",
                CreatedBy = 1,
                TypeId = 1
            };

            var result = await controller.PostCategory(dto);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var value = Assert.IsType<CategoryResponse>(created.Value);

            Assert.Equal("NewCat", value.Name);
            Assert.Equal(1, context.Categories.Count());
        }

        // =========================
        // PUT
        // =========================
        [Fact]
        public async Task PutCategory_ShouldUpdateCategory()
        {
            var context = CreateDb();

            context.Categories.Add(new Category
            {
                CategoryId = 1,
                Name = "Old"
            });
            await context.SaveChangesAsync();

            var repoMock = new Mock<ICategoryRepository>();
            var controller = CreateController(context, repoMock);

            var dto = new UpdateCategoryRequest
            {
                Name = "Updated"
            };

            var result = await controller.PutCategory(1, dto);

            Assert.IsType<OkResult>(result);

            var updated = await context.Categories.FindAsync(1);
            Assert.Equal("Updated", updated.Name);
        }

        // =========================
        // DELETE (Soft)
        // =========================
        [Fact]
        public async Task SoftDeleteCategory_ShouldReturnNoContent()
        {
            var context = CreateDb();

            var repoMock = new Mock<ICategoryRepository>();
            repoMock.Setup(x => x.ExistsAsync(1)).ReturnsAsync(true);

            var controller = CreateController(context, repoMock);

            var result = await controller.SoftDeleteCategory(1);

            Assert.IsType<NoContentResult>(result);
            repoMock.Verify(x => x.ArchiveAsync(1), Times.Once);
        }

        [Fact]
        public async Task SoftDeleteCategory_ShouldReturnNotFound()
        {
            var context = CreateDb();

            var repoMock = new Mock<ICategoryRepository>();
            repoMock.Setup(x => x.ExistsAsync(1)).ReturnsAsync(false);

            var controller = CreateController(context, repoMock);

            var result = await controller.SoftDeleteCategory(1);

            Assert.IsType<NotFoundResult>(result);
        }

        // =========================
        // RESTORE
        // =========================
        [Fact]
        public async Task RestoreCategory_ShouldReturnNoContent()
        {
            var context = CreateDb();

            var repoMock = new Mock<ICategoryRepository>();
            repoMock.Setup(x => x.ExistsAsync(1)).ReturnsAsync(true);

            var controller = CreateController(context, repoMock);

            var result = await controller.RestoreCategory(1);

            Assert.IsType<NoContentResult>(result);
            repoMock.Verify(x => x.RestoreAsync(1), Times.Once);
        }

        // =========================
        // HARD DELETE
        // =========================
        [Fact]
        public async Task HardDeleteCategory_ShouldDelete()
        {
            var context = CreateDb();

            context.Categories.Add(new Category { CategoryId = 1, Name = "ToDelete" });
            await context.SaveChangesAsync();

            var repoMock = new Mock<ICategoryRepository>();
            var controller = CreateController(context, repoMock);

            var result = await controller.HardDeleteCategory(1);

            Assert.IsType<NoContentResult>(result);
            Assert.Empty(context.Categories);
        }
    }
}
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnonymousComplaintsAPI.Services.Implementations;
using AnonymousComplaintsAPI.Repositories.Interfaces;
using AnonymousComplaintsAPI.DTOs.Requests;
using AnonymousComplaintsAPI.DTOs.Responses;
using AnonymousComplaintsAPI.Models.Entities;

namespace TrustLine.Tests.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepo = new();
        private readonly Mock<ILogger<CategoryService>> _logger = new();

        private CategoryService CreateService()
        {
            return new CategoryService(
                _categoryRepo.Object,
                _logger.Object
            );
        }

        // =========================
        // GET ALL
        // =========================

        [Fact]
        public async Task GetAllCategoriesAsync_ShouldReturnCategories()
        {
            var categories = new List<Category>
            {
                new Category { CategoryId = 1, Name = "A" },
                new Category { CategoryId = 2, Name = "B" }
            };

            _categoryRepo.Setup(x => x.GetNonArchivedAsync())
                .ReturnsAsync(categories);

            var service = CreateService();

            var result = await service.GetAllCategoriesAsync();

            Assert.NotNull(result);
        }

        // =========================
        // GET BY ID
        // =========================

        [Fact]
        public async Task GetCategoryAsync_ShouldReturnCategory()
        {
            var category = new Category
            {
                CategoryId = 1,
                Name = "Test"
            };

            _categoryRepo.Setup(x => x.GetWithTypeAsync(1))
                .ReturnsAsync(category);

            var service = CreateService();

            var result = await service.GetCategoryAsync(1);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetCategoryAsync_ShouldReturnNull_WhenNotFound()
        {
            _categoryRepo.Setup(x => x.GetWithTypeAsync(It.IsAny<int>()))
                .ReturnsAsync((Category?)null);

            var service = CreateService();

            var result = await service.GetCategoryAsync(99);

            Assert.Null(result);
        }

        // =========================
        // CREATE
        // =========================

        [Fact]
        public async Task CreateCategory_ShouldReturnCreatedCategory()
        {
            var request = new CategoryResponse
            {
                Name = "New Category",
                TypeId = 1,
                CreatedBy = 5
            };

            _categoryRepo.Setup(x => x.CreateAsync(It.IsAny<Category>()))
                .ReturnsAsync((Category c) =>
                {
                    c.CategoryId = 1;
                    return c;
                });

            var service = CreateService();

            var result = await service.CreateCategoryAsync(request);

            Assert.NotNull(result);
        }

        // =========================
        // UPDATE
        // =========================

        [Fact]
        public async Task UpdateCategory_ShouldUpdateName()
        {
            var category = new Category
            {
                CategoryId = 1,
                Name = "Old"
            };

            _categoryRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(category);

            _categoryRepo.Setup(x => x.UpdateAsync(category))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            var result = await service.UpdateCategoryAsync(1, new UpdateCategoryRequest
            {
                Name = "New"
            });

            Assert.NotNull(result);
            Assert.Equal("New", category.Name);
        }

        [Fact]
        public async Task UpdateCategory_ShouldThrow_WhenNotFound()
        {
            _categoryRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Category?)null);

            var service = CreateService();

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.UpdateCategoryAsync(1, new UpdateCategoryRequest { Name = "X" }));
        }

        // =========================
        // ARCHIVE / RESTORE / DELETE
        // =========================

        [Fact]
        public async Task ArchiveCategory_ShouldCallRepository()
        {
            var service = CreateService();

            await service.ArchiveCategoryAsync(1);

            _categoryRepo.Verify(x => x.ArchiveAsync(1), Times.Once);
        }

        [Fact]
        public async Task RestoreCategory_ShouldCallRepository()
        {
            var service = CreateService();

            await service.RestoreCategoryAsync(1);

            _categoryRepo.Verify(x => x.RestoreAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteCategory_ShouldCallRepository()
        {
            var service = CreateService();

            await service.DeleteCategoryAsync(1);

            _categoryRepo.Verify(x => x.DeleteAsync(1), Times.Once);
        }
    }
}
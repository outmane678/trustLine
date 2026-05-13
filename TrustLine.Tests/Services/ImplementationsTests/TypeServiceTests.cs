using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AnonymousComplaintsAPI.Services.Implementations;
using AnonymousComplaintsAPI.Repositories.Interfaces;
using AnonymousComplaintsAPI.Data;
using AnonymousComplaintsAPI.Models.Entities;
using AnonymousComplaintsAPI.DTOs.Responses;

namespace TrustLine.Tests.Services
{
    public class TypeServiceTests
    {
        private AnonymousComplaintsV002Context CreateDb()
        {
            var options = new DbContextOptionsBuilder<AnonymousComplaintsV002Context>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AnonymousComplaintsV002Context(options);
        }

        private Mock<ITypeRepository> CreateRepo()
        {
            return new Mock<ITypeRepository>();
        }

        private TypeService CreateService(
            AnonymousComplaintsV002Context context,
            ITypeRepository repo)
        {
            var logger = new Mock<ILogger<TypeService>>();

            return new TypeService(repo, context, logger.Object);
        }

        // =========================
        // GET ALL
        // =========================

        [Fact]
        public async Task GetAllTypes_ShouldReturnList()
        {
            var repo = CreateRepo();
            var context = CreateDb();

            repo.Setup(x => x.GetNonArchivedWithCategoriesAsync())
                .ReturnsAsync(new List<AnonymousComplaintsAPI.Models.Entities.Type>
                {
                    new AnonymousComplaintsAPI.Models.Entities.Type { TypeId = 1, Name = "A" },
                    new AnonymousComplaintsAPI.Models.Entities.Type { TypeId = 2, Name = "B" }
                });

            var service = CreateService(context, repo.Object);

            var result = await service.GetAllTypesAsync();

            Assert.Equal(2, result.Count());
        }

        // =========================
        // GET BY ID
        // =========================

        [Fact]
        public async Task GetType_ShouldReturnType()
        {
            var repo = CreateRepo();
            var context = CreateDb();

            repo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new AnonymousComplaintsAPI.Models.Entities.Type
                {
                    TypeId = 1,
                    Name = "Test"
                });

            var service = CreateService(context, repo.Object);

            var result = await service.GetTypeAsync(1, false);

            Assert.NotNull(result);
            Assert.Equal("Test", result!.Name);
        }

        [Fact]
        public async Task GetType_ShouldReturnNull_WhenNotFound()
        {
            var repo = CreateRepo();
            var context = CreateDb();

            repo.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((AnonymousComplaintsAPI.Models.Entities.Type?)null);

            var service = CreateService(context, repo.Object);

            var result = await service.GetTypeAsync(99, false);

            Assert.Null(result);
        }

        // =========================
        // CREATE
        // =========================

        [Fact]
        public async Task CreateType_ShouldReturnCreatedType()
        {
            var repo = CreateRepo();
            var context = CreateDb();

            repo.Setup(x => x.CreateAsync(It.IsAny<AnonymousComplaintsAPI.Models.Entities.Type>()))
                .ReturnsAsync((AnonymousComplaintsAPI.Models.Entities.Type t) =>
                {
                    t.TypeId = 1;
                    return t;
                });

            var service = CreateService(context, repo.Object);

            var dto = new TypeModelResponse
            {
                Name = "New Type"
            };

            var result = await service.CreateTypeAsync(dto);

            Assert.Equal("New Type", result.Name);
        }

        // =========================
        // UPDATE
        // =========================

        [Fact]
        public async Task UpdateType_ShouldUpdateName()
        {
            var repo = CreateRepo();
            var context = CreateDb();

            var entity = new AnonymousComplaintsAPI.Models.Entities.Type
            {
                TypeId = 1,
                Name = "Old"
            };

            repo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(entity);

            repo.Setup(x => x.UpdateAsync(entity))
                .Returns(Task.CompletedTask);

            var service = CreateService(context, repo.Object);

            var dto = new TypeModelResponse
            {
                Name = "New"
            };

            var result = await service.UpdateTypeAsync(1, dto);

            Assert.Equal("New", result.Name);
        }

        [Fact]
        public async Task UpdateType_ShouldThrow_WhenNotFound()
        {
            var repo = CreateRepo();
            var context = CreateDb();

            repo.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((AnonymousComplaintsAPI.Models.Entities.Type?)null);

            var service = CreateService(context, repo.Object);

            var dto = new TypeModelResponse
            {
                Name = "Test"
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.UpdateTypeAsync(1, dto));
        }

        // =========================
        // ARCHIVE + CATEGORIES (IMPORTANT)
        // =========================

        [Fact]
        public async Task ArchiveType_ShouldCallRepository_AndUpdateCategories()
        {
            var repo = CreateRepo();
            var context = CreateDb();

            // seed categories
            context.Categories.Add(new Category { CategoryId = 1, TypeId = 1, Name = "Cat1", Archived = false });
            context.Categories.Add(new Category { CategoryId = 2, TypeId = 1, Name = "Cat2", Archived = false });
            await context.SaveChangesAsync();

            repo.Setup(x => x.ArchiveAsync(1))
                .Returns(Task.CompletedTask);

            var service = CreateService(context, repo.Object);

            await service.ArchiveTypeWithCategoriesAsync(1);

            var cats = context.Categories.Where(c => c.TypeId == 1).ToList();

            Assert.All(cats, c => Assert.True(c.Archived));
        }

        [Fact]
        public async Task RestoreType_ShouldUnarchiveCategories()
        {
            var repo = CreateRepo();
            var context = CreateDb();

            context.Categories.Add(new Category { CategoryId = 1, TypeId = 1, Name = "Cat1", Archived = true });
            await context.SaveChangesAsync();

            repo.Setup(x => x.RestoreAsync(1))
                .Returns(Task.CompletedTask);

            var service = CreateService(context, repo.Object);

            await service.RestoreTypeWithCategoriesAsync(1);

            var cat = context.Categories.First();

            Assert.False(cat.Archived);
        }

        // =========================
        // DELETE
        // =========================

        [Fact]
        public async Task DeleteType_ShouldCallRepository()
        {
            var repo = CreateRepo();
            var context = CreateDb();

            repo.Setup(x => x.DeleteAsync(1))
                .Returns(Task.CompletedTask);

            var service = CreateService(context, repo.Object);

            await service.DeleteTypeAsync(1);

            repo.Verify(x => x.DeleteAsync(1), Times.Once);
        }
    }
}
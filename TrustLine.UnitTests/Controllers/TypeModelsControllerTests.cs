using Xunit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using AnonymousComplaintsAPI.Controllers;
using AnonymousComplaintsAPI.Data;
using AnonymousComplaintsAPI.Models.Entities;
using AnonymousComplaintsAPI.DTOs.Responses;

using TypeEntity = AnonymousComplaintsAPI.Models.Entities.Type;

namespace TrustLine.Tests.Controllers
{
    public class TypeModelsControllerTests
    {
        private AnonymousComplaintsV002Context CreateDb()
        {
            var options = new DbContextOptionsBuilder<AnonymousComplaintsV002Context>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AnonymousComplaintsV002Context(options);
        }

        private TypeModelsController CreateController(AnonymousComplaintsV002Context context)
        {
            return new TypeModelsController(context);
        }

        // ============================
        // ✅ GET ALL TYPES
        // ============================
        [Fact]
        public async Task GetTypes_ReturnsNonArchivedTypes()
        {
            var context = CreateDb();

            context.Types.Add(new TypeEntity { TypeId = 1, Name = "Type1", Archived = false });
            context.Types.Add(new TypeEntity { TypeId = 2, Name = "Type2", Archived = true });
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var result = await controller.GetTypes();

            var types = result.Value;

            Assert.Single(types); // seulement non archivé
            Assert.Equal("Type1", types.First().Name);
        }

        // ============================
        // ✅ GET TYPE BY ID
        // ============================
        [Fact]
        public async Task GetTypeModel_ExistingId_ReturnsType()
        {
            var context = CreateDb();

            context.Types.Add(new TypeEntity { TypeId = 1, Name = "Type1" });
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var result = await controller.GetTypeModel(1);

            Assert.NotNull(result.Value);
            Assert.Equal("Type1", result.Value.Name);
        }

        [Fact]
        public async Task GetTypeModel_NotFound_Returns404()
        {
            var context = CreateDb();
            var controller = CreateController(context);

            var result = await controller.GetTypeModel(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        // ============================
        // ✅ CREATE TYPE
        // ============================
        [Fact]
        public async Task PostTypeModel_CreatesType()
        {
            var context = CreateDb();
            var controller = CreateController(context);

            var dto = new TypeModelResponse
            {
                Name = "NewType",
                CreatedBy = 1,
                CreatedAt = DateTime.Now
            };

            var result = await controller.PostTypeModel(dto);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedDto = Assert.IsType<TypeModelResponse>(created.Value);

            Assert.Equal("NewType", returnedDto.Name);
            Assert.Equal(1, context.Types.Count());
        }

        // ============================
        // ✅ UPDATE TYPE
        // ============================
        [Fact]
        public async Task UpdateTypeModel_UpdatesType()
        {
            var context = CreateDb();

            context.Types.Add(new TypeEntity { TypeId = 1, Name = "Old" });
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var dto = new TypeModelResponse { Name = "Updated" };

            var result = await controller.UpdateTypeModel(1, dto);

            Assert.IsType<OkResult>(result.Result);

            var updated = await context.Types.FindAsync(1);
            Assert.Equal("Updated", updated.Name);
        }

        // ============================
        // ❌ UPDATE NOT FOUND
        // ============================
        [Fact]
        public async Task UpdateTypeModel_NotFound_Returns404()
        {
            var context = CreateDb();
            var controller = CreateController(context);

            var dto = new TypeModelResponse { Name = "Updated" };

            var result = await controller.UpdateTypeModel(1, dto);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        // ============================
        // ✅ ARCHIVE TYPE
        // ============================
        [Fact]
        public async Task ArchiveTypeModel_ArchivesTypeAndCategories()
        {
            var context = CreateDb();

            context.Types.Add(new TypeEntity { TypeId = 1, Name = "Type1", Archived = false });
            context.Categories.Add(new Category { CategoryId = 1, TypeId = 1, Name = "Cat1", Archived = false });

            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var result = await controller.ArchiveTypeModel(1);

            Assert.IsType<NoContentResult>(result);

            var type = await context.Types.FindAsync(1);
            var category = await context.Categories.FirstAsync();

            Assert.True(type.Archived);
            Assert.True(category.Archived);
        }

        // ============================
        // ✅ RESTORE TYPE
        // ============================
        [Fact]
        public async Task RestoreTypeModel_RestoresTypeAndCategories()
        {
            var context = CreateDb();

            context.Types.Add(new TypeEntity { TypeId = 1, Name = "Type1", Archived = true });
            context.Categories.Add(new Category { CategoryId = 1, TypeId = 1, Name = "Cat1", Archived = true });

            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var result = await controller.RestoreTypeModel(1);

            Assert.IsType<NoContentResult>(result);

            var type = await context.Types.FindAsync(1);
            var category = await context.Categories.FirstAsync();

            Assert.False(type.Archived);
            Assert.False(category.Archived);
        }

        // ============================
        // ✅ DELETE TYPE (soft delete)
        // ============================
        [Fact]
        public async Task DeleteTypeModel_SetsArchivedTrue()
        {
            var context = CreateDb();

            context.Types.Add(new TypeEntity { TypeId = 1, Name = "Type1", Archived = false });
            context.Categories.Add(new Category { CategoryId = 1, TypeId = 1, Name = "Cat1", Archived = false });

            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var result = await controller.DeleteTypeModel(1);

            Assert.IsType<NoContentResult>(result);

            var type = await context.Types.FindAsync(1);
            Assert.True(type.Archived);
        }
    }
}
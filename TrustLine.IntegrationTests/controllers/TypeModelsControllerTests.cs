using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

using AnonymousComplaintsAPI.Data;
using AnonymousComplaintsAPI.DTOs.Responses;
using AnonymousComplaintsAPI.Models.Entities;
using TrustLine.IntegrationTests.Helpers;

using TypeEntity = AnonymousComplaintsAPI.Models.Entities.Type;

namespace TrustLine.IntegrationTests.Controllers
{
    public class TypeModelsControllerTests
        : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private AnonymousComplaintsV002Context _context = null!;
        private IServiceScope _scope = null!;

        public TypeModelsControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        public Task InitializeAsync()
        {
            _scope = _factory.Services.CreateScope();
            _context = _scope.ServiceProvider
                .GetRequiredService<AnonymousComplaintsV002Context>();
            _context.Database.EnsureCreated();

            _context.Categories.RemoveRange(_context.Categories);
            _context.Types.RemoveRange(_context.Types);
            _context.SaveChanges();

            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            _context.Dispose();
            _scope.Dispose();
            return Task.CompletedTask;
        }

        // ================================================================
        // Helpers
        // ================================================================
        private async Task<TypeEntity> SeedTypeAsync(
            int id, string name = "Test Type",
            bool archived = false)
        {
            var type = new TypeEntity
            {
                TypeId = id,
                Name = name,
                Archived = archived,
                CreatedAt = DateTime.Now
            };
            _context.Types.Add(type);
            await _context.SaveChangesAsync();
            return type;
        }

        private async Task SeedTypeWithCategoriesAsync(int typeId, string typeName, params string[] categoryNames)
        {
            var type = new TypeEntity
            {
                TypeId = typeId,
                Name = typeName,
                Archived = false,
                CreatedAt = DateTime.Now
            };
            _context.Types.Add(type);

            int catId = typeId * 100;
            foreach (var catName in categoryNames)
            {
                _context.Categories.Add(new Category
                {
                    CategoryId = catId++,
                    Name = catName,
                    TypeId = typeId,
                    Archived = false
                });
            }
            await _context.SaveChangesAsync();
        }

        // ================================================================
        // GET (non-archived) - GET /api/TypeModels
        // ================================================================
        [Fact]
        public async Task GetTypes_ReturnsOk_WithEmptyList()
        {
            var response = await _client.GetAsync("/api/TypeModels");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetTypes_FiltersArchived()
        {
            await SeedTypeAsync(100, "Active Type", archived: false);
            await SeedTypeAsync(101, "Archived Type", archived: true);

            var response = await _client.GetAsync("/api/TypeModels");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Active Type");
            content.Should().NotContain("Archived Type");
        }

        [Fact]
        public async Task GetTypes_IncludesCategories()
        {
            await SeedTypeWithCategoriesAsync(110, "Type With Cats", "Cat A", "Cat B");

            var response = await _client.GetAsync("/api/TypeModels");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Type With Cats");
            content.Should().Contain("Cat A");
            content.Should().Contain("Cat B");
        }

        // ================================================================
        // GET ALL - GET /api/TypeModels/all
        // ================================================================
        [Fact]
        public async Task GetAllTypes_IncludesArchived()
        {
            await SeedTypeAsync(120, "Active", archived: false);
            await SeedTypeAsync(121, "Archived", archived: true);

            var response = await _client.GetAsync("/api/TypeModels/all");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Active");
            content.Should().Contain("Archived");
        }

        // ================================================================
        // GET BY ID - GET /api/TypeModels/{id}
        // ================================================================
        [Fact]
        public async Task GetTypeModel_Existing_ReturnsOk()
        {
            await SeedTypeAsync(200, "My Type");

            var response = await _client.GetAsync("/api/TypeModels/200");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("My Type");
        }

        [Fact]
        public async Task GetTypeModel_NonExistent_ReturnsNotFound()
        {
            var response = await _client.GetAsync("/api/TypeModels/9999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // CREATE - POST /api/TypeModels
        // ================================================================
        [Fact]
        public async Task PostTypeModel_ValidData_ReturnsCreated()
        {
            var dto = new TypeModelResponse
            {
                Name = "New Type",
                Archived = false,
                CreatedAt = DateTime.Now
            };

            var response = await _client.PostAsJsonAsync("/api/TypeModels", dto);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("New Type");
        }

        // ================================================================
        // UPDATE - PUT /api/TypeModels/{id}
        // ================================================================
        [Fact]
        public async Task PutTypeModel_ValidData_ReturnsOk()
        {
            await SeedTypeAsync(300, "Old Type Name");

            var dto = new TypeModelResponse
            {
                TypeId = 300,
                Name = "Updated Type Name"
            };

            var response = await _client.PutAsJsonAsync("/api/TypeModels/300", dto);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var type = await _context.Types.FindAsync(300);
            await _context.Entry(type!).ReloadAsync();
            type.Name.Should().Be("Updated Type Name");
        }

        [Fact]
        public async Task PutTypeModel_NotFound_ReturnsNotFound()
        {
            var dto = new TypeModelResponse { TypeId = 9999, Name = "Ghost" };

            var response = await _client.PutAsJsonAsync("/api/TypeModels/9999", dto);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // ARCHIVE - PATCH /api/TypeModels/archive/{id}
        // ================================================================
        [Fact]
        public async Task ArchiveTypeModel_ArchivesTypeAndCategories()
        {
            await SeedTypeWithCategoriesAsync(400, "To Archive", "Child Cat 1", "Child Cat 2");

            var response = await _client.PatchAsync("/api/TypeModels/archive/400", null);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify type archived
            var type = await _context.Types.FindAsync(400);
            await _context.Entry(type!).ReloadAsync();
            type.Archived.Should().BeTrue();

            // Verify categories also archived
            var cats = await _context.Categories.Where(c => c.TypeId == 400).ToListAsync();
            foreach (var cat in cats)
            {
                await _context.Entry(cat).ReloadAsync();
                cat.Archived.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ArchiveTypeModel_NotFound_ReturnsNotFound()
        {
            var response = await _client.PatchAsync("/api/TypeModels/archive/9999", null);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // RESTORE - PUT /api/TypeModels/restore/{id}
        // ================================================================
        [Fact]
        public async Task RestoreTypeModel_RestoresTypeAndCategories()
        {
            await SeedTypeAsync(500, "Archived Type", archived: true);
            _context.Categories.Add(new Category
            {
                CategoryId = 5001,
                Name = "Archived Cat",
                TypeId = 500,
                Archived = true
            });
            await _context.SaveChangesAsync();

            var response = await _client.PutAsync("/api/TypeModels/restore/500", null);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var type = await _context.Types.FindAsync(500);
            await _context.Entry(type!).ReloadAsync();
            type.Archived.Should().BeFalse();

            var cat = await _context.Categories.FindAsync(5001);
            await _context.Entry(cat!).ReloadAsync();
            cat.Archived.Should().BeFalse();
        }

        [Fact]
        public async Task RestoreTypeModel_NotFound_ReturnsNotFound()
        {
            var response = await _client.PutAsync("/api/TypeModels/restore/9999", null);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // DELETE (soft) - DELETE /api/TypeModels/{id}
        // ================================================================
        [Fact]
        public async Task DeleteTypeModel_ArchivesTypeAndCategories()
        {
            await SeedTypeWithCategoriesAsync(600, "To Delete", "Cat To Del");

            var response = await _client.DeleteAsync("/api/TypeModels/600");
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var type = await _context.Types.FindAsync(600);
            await _context.Entry(type!).ReloadAsync();
            type.Archived.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteTypeModel_NotFound_ReturnsNotFound()
        {
            var response = await _client.DeleteAsync("/api/TypeModels/9999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // END-TO-END
        // ================================================================
        [Fact]
        public async Task FullLifecycle_Create_Update_Archive_Restore_Delete()
        {
            // 1. Create
            var createDto = new TypeModelResponse
            {
                Name = "Lifecycle Type",
                Archived = false,
                CreatedAt = DateTime.Now
            };
            var createResp = await _client.PostAsJsonAsync("/api/TypeModels", createDto);
            createResp.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await createResp.Content.ReadFromJsonAsync<TypeModelResponse>();
            var id = created!.TypeId;

            // 2. Get
            var getResp = await _client.GetAsync($"/api/TypeModels/{id}");
            getResp.StatusCode.Should().Be(HttpStatusCode.OK);

            // 3. Update
            var updateDto = new TypeModelResponse { TypeId = id, Name = "Updated Lifecycle" };
            var updateResp = await _client.PutAsJsonAsync($"/api/TypeModels/{id}", updateDto);
            updateResp.StatusCode.Should().Be(HttpStatusCode.OK);

            // 4. Archive
            var archiveResp = await _client.PatchAsync($"/api/TypeModels/archive/{id}", null);
            archiveResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 5. Restore
            var restoreResp = await _client.PutAsync($"/api/TypeModels/restore/{id}", null);
            restoreResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 6. Delete (soft)
            var deleteResp = await _client.DeleteAsync($"/api/TypeModels/{id}");
            deleteResp.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}

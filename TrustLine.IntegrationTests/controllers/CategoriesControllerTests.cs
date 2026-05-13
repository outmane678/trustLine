using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

using AnonymousComplaintsAPI.Data;
using AnonymousComplaintsAPI.DTOs.Requests;
using AnonymousComplaintsAPI.DTOs.Responses;
using AnonymousComplaintsAPI.Models.Entities;
using TrustLine.IntegrationTests.Helpers;

using TypeEntity = AnonymousComplaintsAPI.Models.Entities.Type;

namespace TrustLine.IntegrationTests.Controllers
{
    public class CategoriesControllerTests
        : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private AnonymousComplaintsV002Context _context = null!;
        private IServiceScope _scope = null!;

        public CategoriesControllerTests(CustomWebApplicationFactory factory)
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
        private async Task<TypeEntity> SeedTypeAsync(int id = 1, string name = "Test Type")
        {
            var type = new TypeEntity { TypeId = id, Name = name, Archived = false };
            _context.Types.Add(type);
            await _context.SaveChangesAsync();
            return type;
        }

        private async Task<Category> SeedCategoryAsync(
            int id, string name = "Test Category",
            bool archived = false, int? typeId = null)
        {
            var cat = new Category
            {
                CategoryId = id,
                Name = name,
                Archived = archived,
                TypeId = typeId,
                CreatedAt = DateTime.Now
            };
            _context.Categories.Add(cat);
            await _context.SaveChangesAsync();
            return cat;
        }

        // ================================================================
        // GET ALL - GET /api/Categories
        // ================================================================
        [Fact]
        public async Task GetCategories_ReturnsOk_WithEmptyList()
        {
            var response = await _client.GetAsync("/api/Categories");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetCategories_ReturnsOk_WithData()
        {
            await SeedCategoryAsync(100, "Category A");
            await SeedCategoryAsync(101, "Category B");

            var response = await _client.GetAsync("/api/Categories");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Category A");
            content.Should().Contain("Category B");
        }

        // ================================================================
        // GET BY TYPE - GET /api/Categories/byType/{typeId}
        // ================================================================
        [Fact]
        public async Task GetCategoriesByType_ReturnsFilteredCategories()
        {
            var type = await SeedTypeAsync(10, "Type X");
            await SeedCategoryAsync(110, "Cat for Type X", typeId: 10);
            await SeedCategoryAsync(111, "Cat for other", typeId: 99);

            var response = await _client.GetAsync("/api/Categories/byType/10");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Cat for Type X");
            content.Should().NotContain("Cat for other");
        }

        [Fact]
        public async Task GetCategoriesByType_NoMatch_ReturnsEmptyList()
        {
            var response = await _client.GetAsync("/api/Categories/byType/9999");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Be("[]");
        }

        // ================================================================
        // GET BY ID - GET /api/Categories/{id}
        // ================================================================
        [Fact]
        public async Task GetCategory_Existing_ReturnsOk()
        {
            await SeedCategoryAsync(200, "My Category");

            var response = await _client.GetAsync("/api/Categories/200");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("My Category");
        }

        [Fact]
        public async Task GetCategory_NonExistent_ReturnsNotFound()
        {
            var response = await _client.GetAsync("/api/Categories/9999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // CREATE - POST /api/Categories
        // ================================================================
        [Fact]
        public async Task PostCategory_ValidData_ReturnsCreated()
        {
            var dto = new CategoryResponse
            {
                Name = "New Cat",
                Archived = false,
                CreatedAt = DateTime.Now
            };

            var response = await _client.PostAsJsonAsync("/api/Categories", dto);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("New Cat");
        }

        [Fact]
        public async Task PostCategory_WithTypeId_ReturnsCreated()
        {
            await SeedTypeAsync(20, "Parent Type");

            var dto = new CategoryResponse
            {
                Name = "Typed Cat",
                TypeId = 20,
                Archived = false
            };

            var response = await _client.PostAsJsonAsync("/api/Categories", dto);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // ================================================================
        // UPDATE - PUT /api/Categories/{id}
        // ================================================================
        [Fact]
        public async Task PutCategory_ValidData_ReturnsOk()
        {
            await SeedCategoryAsync(300, "Old Name");

            var dto = new UpdateCategoryRequest { Name = "Updated Name" };

            var response = await _client.PutAsJsonAsync("/api/Categories/300", dto);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify update
            var cat = await _context.Categories.FindAsync(300);
            await _context.Entry(cat!).ReloadAsync();
            cat.Name.Should().Be("Updated Name");
        }

        [Fact]
        public async Task PutCategory_NotFound_ReturnsNotFound()
        {
            var dto = new UpdateCategoryRequest { Name = "Ghost" };

            var response = await _client.PutAsJsonAsync("/api/Categories/9999", dto);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // SOFT DELETE - DELETE /api/Categories/{id}
        // ================================================================
        [Fact]
        public async Task SoftDeleteCategory_Existing_ReturnsNoContent()
        {
            await SeedCategoryAsync(400, "To Archive");

            var response = await _client.DeleteAsync("/api/Categories/400");
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var cat = await _context.Categories.FindAsync(400);
            await _context.Entry(cat!).ReloadAsync();
            cat.Archived.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteCategory_NotFound_ReturnsNotFound()
        {
            var response = await _client.DeleteAsync("/api/Categories/9999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // RESTORE - PUT /api/Categories/restore/{id}
        // ================================================================
        [Fact]
        public async Task RestoreCategory_Archived_ReturnsNoContent()
        {
            await SeedCategoryAsync(500, "Archived Cat", archived: true);

            var response = await _client.PutAsync("/api/Categories/restore/500", null);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var cat = await _context.Categories.FindAsync(500);
            await _context.Entry(cat!).ReloadAsync();
            cat.Archived.Should().BeFalse();
        }

        [Fact]
        public async Task RestoreCategory_NotFound_ReturnsNotFound()
        {
            var response = await _client.PutAsync("/api/Categories/restore/9999", null);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // HARD DELETE - DELETE /api/Categories/hard-delete/{id}
        // ================================================================
        [Fact]
        public async Task HardDeleteCategory_Existing_ReturnsNoContent()
        {
            await SeedCategoryAsync(600, "To Delete");

            var response = await _client.DeleteAsync("/api/Categories/hard-delete/600");
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var local = _context.Categories.Local.FirstOrDefault(c => c.CategoryId == 600);
            if (local != null)
                _context.Entry(local).State = EntityState.Detached;

            var cat = await _context.Categories.FindAsync(600);
            cat.Should().BeNull();
        }

        [Fact]
        public async Task HardDeleteCategory_NotFound_ReturnsNotFound()
        {
            var response = await _client.DeleteAsync("/api/Categories/hard-delete/9999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // END-TO-END
        // ================================================================
        [Fact]
        public async Task FullLifecycle_Create_Update_Archive_Restore_HardDelete()
        {
            // 1. Create
            var createDto = new CategoryResponse
            {
                Name = "Lifecycle Cat",
                Archived = false,
                CreatedAt = DateTime.Now
            };
            var createResp = await _client.PostAsJsonAsync("/api/Categories", createDto);
            createResp.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await createResp.Content.ReadFromJsonAsync<CategoryResponse>();
            var id = created!.Id;

            // 2. Get
            var getResp = await _client.GetAsync($"/api/Categories/{id}");
            getResp.StatusCode.Should().Be(HttpStatusCode.OK);

            // 3. Update
            var updateDto = new UpdateCategoryRequest { Name = "Updated Lifecycle" };
            var updateResp = await _client.PutAsJsonAsync($"/api/Categories/{id}", updateDto);
            updateResp.StatusCode.Should().Be(HttpStatusCode.OK);

            // 4. Archive
            var archiveResp = await _client.DeleteAsync($"/api/Categories/{id}");
            archiveResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 5. Restore
            var restoreResp = await _client.PutAsync($"/api/Categories/restore/{id}", null);
            restoreResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 6. Hard delete
            var hardDeleteResp = await _client.DeleteAsync($"/api/Categories/hard-delete/{id}");
            hardDeleteResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 7. Verify gone
            var goneResp = await _client.GetAsync($"/api/Categories/{id}");
            goneResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}

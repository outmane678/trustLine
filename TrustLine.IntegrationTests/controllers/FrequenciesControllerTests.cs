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

namespace TrustLine.IntegrationTests.Controllers
{
    public class FrequenciesControllerTests
        : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private AnonymousComplaintsV002Context _context = null!;
        private IServiceScope _scope = null!;

        public FrequenciesControllerTests(CustomWebApplicationFactory factory)
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

            _context.Frequencies.RemoveRange(_context.Frequencies);
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
        private async Task<Frequency> SeedFrequencyAsync(
            int id, string label = "Weekly",
            bool archived = false)
        {
            var freq = new Frequency
            {
                FrequencyId = id,
                Label = label,
                Archived = archived,
                CreatedAt = DateTime.Now
            };
            _context.Frequencies.Add(freq);
            await _context.SaveChangesAsync();
            return freq;
        }

        // ================================================================
        // GET ALL - GET /api/Frequencies
        // ================================================================
        [Fact]
        public async Task GetFrequencies_ReturnsOk_WithEmptyList()
        {
            var response = await _client.GetAsync("/api/Frequencies");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetFrequencies_ReturnsAllIncludingArchived()
        {
            await SeedFrequencyAsync(100, "Daily", archived: false);
            await SeedFrequencyAsync(101, "Monthly", archived: true);

            var response = await _client.GetAsync("/api/Frequencies");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Daily");
            content.Should().Contain("Monthly");
        }

        // ================================================================
        // GET BY ID - GET /api/Frequencies/{id}
        // ================================================================
        [Fact]
        public async Task GetFrequency_Existing_ReturnsOk()
        {
            await SeedFrequencyAsync(200, "Weekly");

            var response = await _client.GetAsync("/api/Frequencies/200");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Weekly");
        }

        [Fact]
        public async Task GetFrequency_NonExistent_ReturnsNotFound()
        {
            var response = await _client.GetAsync("/api/Frequencies/9999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // CREATE - POST /api/Frequencies
        // ================================================================
        [Fact]
        public async Task PostFrequency_ValidData_ReturnsCreated()
        {
            var dto = new FrequencyResponse
            {
                Label = "Annually",
                Archived = false,
                CreatedAt = DateTime.Now
            };

            var response = await _client.PostAsJsonAsync("/api/Frequencies", dto);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Annually");
        }

        // ================================================================
        // UPDATE - PUT /api/Frequencies/{id}
        // ================================================================
        [Fact]
        public async Task PutFrequency_ValidData_ReturnsNoContent()
        {
            await SeedFrequencyAsync(300, "Old Label");

            var dto = new FrequencyResponse
            {
                FrequencyID = 300,
                Label = "Updated Label",
                Archived = false
            };

            var response = await _client.PutAsJsonAsync("/api/Frequencies/300", dto);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var freq = await _context.Frequencies.FindAsync(300);
            await _context.Entry(freq!).ReloadAsync();
            freq.Label.Should().Be("Updated Label");
        }

        [Fact]
        public async Task PutFrequency_IdMismatch_ReturnsBadRequest()
        {
            await SeedFrequencyAsync(301, "Label");

            var dto = new FrequencyResponse
            {
                FrequencyID = 999,
                Label = "Mismatch"
            };

            var response = await _client.PutAsJsonAsync("/api/Frequencies/301", dto);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PutFrequency_NotFound_ReturnsNotFound()
        {
            var dto = new FrequencyResponse
            {
                FrequencyID = 9999,
                Label = "Ghost"
            };

            var response = await _client.PutAsJsonAsync("/api/Frequencies/9999", dto);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // ARCHIVE - PATCH /api/Frequencies/archive/{id}
        // ================================================================
        [Fact]
        public async Task ArchiveFrequency_Existing_ReturnsNoContent()
        {
            await SeedFrequencyAsync(400, "To Archive");

            var response = await _client.PatchAsync("/api/Frequencies/archive/400", null);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var freq = await _context.Frequencies.FindAsync(400);
            await _context.Entry(freq!).ReloadAsync();
            freq.Archived.Should().BeTrue();
        }

        [Fact]
        public async Task ArchiveFrequency_NotFound_ReturnsNotFound()
        {
            var response = await _client.PatchAsync("/api/Frequencies/archive/9999", null);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // RESTORE - PATCH /api/Frequencies/restore/{id}
        // ================================================================
        [Fact]
        public async Task RestoreFrequency_Archived_ReturnsNoContent()
        {
            await SeedFrequencyAsync(500, "Archived Freq", archived: true);

            var response = await _client.PatchAsync("/api/Frequencies/restore/500", null);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var freq = await _context.Frequencies.FindAsync(500);
            await _context.Entry(freq!).ReloadAsync();
            freq.Archived.Should().BeFalse();
        }

        [Fact]
        public async Task RestoreFrequency_NotFound_ReturnsNotFound()
        {
            var response = await _client.PatchAsync("/api/Frequencies/restore/9999", null);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // HARD DELETE - DELETE /api/Frequencies/{id}
        // ================================================================
        [Fact]
        public async Task DeleteFrequency_Existing_ReturnsNoContent()
        {
            await SeedFrequencyAsync(600, "To Delete");

            var response = await _client.DeleteAsync("/api/Frequencies/600");
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var local = _context.Frequencies.Local.FirstOrDefault(f => f.FrequencyId == 600);
            if (local != null)
                _context.Entry(local).State = EntityState.Detached;

            var freq = await _context.Frequencies.FindAsync(600);
            freq.Should().BeNull();
        }

        [Fact]
        public async Task DeleteFrequency_NotFound_ReturnsNotFound()
        {
            var response = await _client.DeleteAsync("/api/Frequencies/9999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // END-TO-END
        // ================================================================
        [Fact]
        public async Task FullLifecycle_Create_Update_Archive_Restore_Delete()
        {
            // 1. Create
            var createDto = new FrequencyResponse
            {
                Label = "Lifecycle Freq",
                Archived = false,
                CreatedAt = DateTime.Now
            };
            var createResp = await _client.PostAsJsonAsync("/api/Frequencies", createDto);
            createResp.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await createResp.Content.ReadFromJsonAsync<FrequencyResponse>();
            var id = created!.FrequencyID;

            // 2. Get
            var getResp = await _client.GetAsync($"/api/Frequencies/{id}");
            getResp.StatusCode.Should().Be(HttpStatusCode.OK);

            // 3. Update
            var updateDto = new FrequencyResponse
            {
                FrequencyID = id,
                Label = "Updated Lifecycle",
                Archived = false
            };
            var updateResp = await _client.PutAsJsonAsync($"/api/Frequencies/{id}", updateDto);
            updateResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 4. Archive
            var archiveResp = await _client.PatchAsync($"/api/Frequencies/archive/{id}", null);
            archiveResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 5. Restore
            var restoreResp = await _client.PatchAsync($"/api/Frequencies/restore/{id}", null);
            restoreResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 6. Hard delete
            var deleteResp = await _client.DeleteAsync($"/api/Frequencies/{id}");
            deleteResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 7. Verify gone
            var goneResp = await _client.GetAsync($"/api/Frequencies/{id}");
            goneResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}

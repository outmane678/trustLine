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
    public class SolutionsControllerTests
        : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private AnonymousComplaintsV002Context _context = null!;
        private IServiceScope _scope = null!;

        public SolutionsControllerTests(CustomWebApplicationFactory factory)
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

            _context.Solutions.RemoveRange(_context.Solutions);
            _context.AnonymousComplaints.RemoveRange(_context.AnonymousComplaints);
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
        private async Task<Solution> SeedSolutionAsync(
            int id, string content = "Solution content",
            bool archived = false, int? complaintId = null)
        {
            var sol = new Solution
            {
                SolutionId = id,
                Content = content,
                Archived = archived,
                AnonymousComplaintId = complaintId,
                CreatedAt = DateTime.Now
            };
            _context.Solutions.Add(sol);
            await _context.SaveChangesAsync();
            return sol;
        }

        private async Task<AnonymousComplaint> SeedComplaintAsync(
            int id, string state = "SUBMITTED")
        {
            var complaint = new AnonymousComplaint
            {
                AnonymousComplaintId = id,
                Description = "Complaint " + id,
                State = state,
                Archived = false,
                IsIdentityVisible = false
            };
            _context.AnonymousComplaints.Add(complaint);
            await _context.SaveChangesAsync();
            return complaint;
        }

        // ================================================================
        // GET (non-archived) - GET /api/Solutions
        // ================================================================
        [Fact]
        public async Task GetSolutions_ReturnsOk_WithEmptyList()
        {
            var response = await _client.GetAsync("/api/Solutions");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetSolutions_FiltersArchivedSolutions()
        {
            await SeedSolutionAsync(100, "Active solution", archived: false);
            await SeedSolutionAsync(101, "Archived solution", archived: true);

            var response = await _client.GetAsync("/api/Solutions");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Active solution");
            content.Should().NotContain("Archived solution");
        }

        // ================================================================
        // GET ALL - GET /api/Solutions/all
        // ================================================================
        [Fact]
        public async Task GetAllSolutions_IncludesArchived()
        {
            await SeedSolutionAsync(110, "Active", archived: false);
            await SeedSolutionAsync(111, "Archived", archived: true);

            var response = await _client.GetAsync("/api/Solutions/all");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Active");
            content.Should().Contain("Archived");
        }

        // ================================================================
        // GET BY ID - GET /api/Solutions/{id}
        // ================================================================
        [Fact]
        public async Task GetSolution_Existing_ReturnsOk()
        {
            await SeedSolutionAsync(200, "My solution");

            var response = await _client.GetAsync("/api/Solutions/200");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("My solution");
        }

        [Fact]
        public async Task GetSolution_NonExistent_ReturnsNotFound()
        {
            var response = await _client.GetAsync("/api/Solutions/9999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // CREATE - POST /api/Solutions
        // ================================================================
        [Fact]
        public async Task PostSolution_ValidData_ReturnsCreated()
        {
            await SeedComplaintAsync(300, "IN PROGRESS");

            var dto = new SolutionResponse
            {
                Content = "This is a brand new solution for the complaint",
                AnonymousComplaintID = 300,
                Archived = false,
                CreatedAt = DateTime.Now
            };

            var response = await _client.PostAsJsonAsync("/api/Solutions", dto);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            // Verify complaint state changed to RESOLVED
            var complaint = await _context.AnonymousComplaints.FindAsync(300);
            await _context.Entry(complaint!).ReloadAsync();
            complaint.State.Should().Be("RESOLVED");
        }

        [Fact]
        public async Task PostSolution_WithMergedComplaints_ResolvesAll()
        {
            // Main complaint
            await SeedComplaintAsync(310, "IN PROGRESS");
            // Fused complaint
            var fused = new AnonymousComplaint
            {
                AnonymousComplaintId = 311,
                Description = "Fused complaint",
                State = "IN PROGRESS",
                Archived = false,
                FusionWithId = 310,
                IsIdentityVisible = false
            };
            _context.AnonymousComplaints.Add(fused);
            await _context.SaveChangesAsync();

            var dto = new SolutionResponse
            {
                Content = "This is the solution for the merged complaint",
                AnonymousComplaintID = 310,
                Archived = false,
                CreatedAt = DateTime.Now
            };

            var response = await _client.PostAsJsonAsync("/api/Solutions", dto);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            // Verify both complaints resolved
            var main = await _context.AnonymousComplaints.FindAsync(310);
            await _context.Entry(main!).ReloadAsync();
            main.State.Should().Be("RESOLVED");

            var fusedResult = await _context.AnonymousComplaints.FindAsync(311);
            await _context.Entry(fusedResult!).ReloadAsync();
            fusedResult.State.Should().Be("RESOLVED");

            // Verify solution created for fused complaint too
            var fusedSolutions = await _context.Solutions
                .Where(s => s.AnonymousComplaintId == 311)
                .ToListAsync();
            fusedSolutions.Should().HaveCountGreaterThan(0);
        }

        // ================================================================
        // UPDATE - PUT /api/Solutions/{id}
        // ================================================================
        [Fact]
        public async Task PutSolution_ValidData_ReturnsNoContent()
        {
            await SeedSolutionAsync(400, "Old content");

            var dto = new SolutionResponse
            {
                SolutionID = 400,
                Content = "This is the updated content for solution",
                Archived = false
            };

            var response = await _client.PutAsJsonAsync("/api/Solutions/400", dto);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var sol = await _context.Solutions.FindAsync(400);
            await _context.Entry(sol!).ReloadAsync();
            sol.Content.Should().Be("This is the updated content for solution");
        }

        [Fact]
        public async Task PutSolution_IdMismatch_ReturnsBadRequest()
        {
            await SeedSolutionAsync(401, "Content");

            var dto = new SolutionResponse
            {
                SolutionID = 999,
                Content = "This is a mismatch solution content here"
            };

            var response = await _client.PutAsJsonAsync("/api/Solutions/401", dto);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PutSolution_NotFound_ReturnsNotFound()
        {
            var dto = new SolutionResponse
            {
                SolutionID = 9999,
                Content = "This is a ghost solution content here"
            };

            var response = await _client.PutAsJsonAsync("/api/Solutions/9999", dto);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // ARCHIVE - PATCH /api/Solutions/archive/{id}
        // ================================================================
        [Fact]
        public async Task ArchiveSolution_Existing_ReturnsNoContent()
        {
            await SeedSolutionAsync(500, "To archive");

            var response = await _client.PatchAsync("/api/Solutions/archive/500", null);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var sol = await _context.Solutions.FindAsync(500);
            await _context.Entry(sol!).ReloadAsync();
            sol.Archived.Should().BeTrue();
        }

        [Fact]
        public async Task ArchiveSolution_NotFound_ReturnsNotFound()
        {
            var response = await _client.PatchAsync("/api/Solutions/archive/9999", null);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // RESTORE - PATCH /api/Solutions/restore/{id}
        // ================================================================
        [Fact]
        public async Task RestoreSolution_Archived_ReturnsNoContent()
        {
            await SeedSolutionAsync(600, "Archived sol", archived: true);

            var response = await _client.PatchAsync("/api/Solutions/restore/600", null);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var sol = await _context.Solutions.FindAsync(600);
            await _context.Entry(sol!).ReloadAsync();
            sol.Archived.Should().BeFalse();
        }

        [Fact]
        public async Task RestoreSolution_NotFound_ReturnsNotFound()
        {
            var response = await _client.PatchAsync("/api/Solutions/restore/9999", null);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // HARD DELETE - DELETE /api/Solutions/{id}
        // ================================================================
        [Fact]
        public async Task DeleteSolution_Existing_ReturnsNoContent()
        {
            await SeedSolutionAsync(700, "To hard delete");

            var response = await _client.DeleteAsync("/api/Solutions/700");
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var local = _context.Solutions.Local.FirstOrDefault(s => s.SolutionId == 700);
            if (local != null)
                _context.Entry(local).State = EntityState.Detached;

            var sol = await _context.Solutions.FindAsync(700);
            sol.Should().BeNull();
        }

        [Fact]
        public async Task DeleteSolution_NotFound_ReturnsNotFound()
        {
            var response = await _client.DeleteAsync("/api/Solutions/9999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // END-TO-END
        // ================================================================
        [Fact]
        public async Task FullLifecycle_Create_Update_Archive_Restore_Delete()
        {
            await SeedComplaintAsync(800);

            // 1. Create
            var createDto = new SolutionResponse
            {
                Content = "This is a lifecycle solution content here",
                AnonymousComplaintID = 800,
                Archived = false,
                CreatedAt = DateTime.Now
            };
            var createResp = await _client.PostAsJsonAsync("/api/Solutions", createDto);
            createResp.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await createResp.Content.ReadFromJsonAsync<SolutionResponse>();
            var id = created!.SolutionID;

            // 2. Get
            var getResp = await _client.GetAsync($"/api/Solutions/{id}");
            getResp.StatusCode.Should().Be(HttpStatusCode.OK);

            // 3. Update
            var updateDto = new SolutionResponse
            {
                SolutionID = id,
                Content = "Updated lifecycle solution content text",
                Archived = false
            };
            var updateResp = await _client.PutAsJsonAsync($"/api/Solutions/{id}", updateDto);
            updateResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 4. Archive
            var archiveResp = await _client.PatchAsync($"/api/Solutions/archive/{id}", null);
            archiveResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 5. Restore
            var restoreResp = await _client.PatchAsync($"/api/Solutions/restore/{id}", null);
            restoreResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 6. Hard delete
            var deleteResp = await _client.DeleteAsync($"/api/Solutions/{id}");
            deleteResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 7. Verify gone
            var goneResp = await _client.GetAsync($"/api/Solutions/{id}");
            goneResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}

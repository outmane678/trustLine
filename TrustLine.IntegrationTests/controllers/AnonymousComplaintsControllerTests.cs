using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

using AnonymousComplaintsAPI.Data;
using AnonymousComplaintsAPI.DTOs.Requests;
using AnonymousComplaintsAPI.Models.Entities;
using TrustLine.IntegrationTests.Helpers;

using TypeEntity = AnonymousComplaintsAPI.Models.Entities.Type;

namespace TrustLine.IntegrationTests.Controllers
{
    public class AnonymousComplaintsControllerTests
        : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private AnonymousComplaintsV002Context _context = null!;
        private IServiceScope _scope = null!;

        public AnonymousComplaintsControllerTests(CustomWebApplicationFactory factory)
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

            // Clean all data before each test
            _context.Solutions.RemoveRange(_context.Solutions);
            _context.Attachments.RemoveRange(_context.Attachments);
            _context.AnonymousComplaints.RemoveRange(_context.AnonymousComplaints);
            _context.Categories.RemoveRange(_context.Categories);
            _context.Types.RemoveRange(_context.Types);
            _context.Frequencies.RemoveRange(_context.Frequencies);
            _context.Users.RemoveRange(_context.Users);
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
        // Helper: seed common data
        // ================================================================
        private async Task<(User user, TypeEntity type)> SeedUserAndTypeAsync(int userId = 1, int typeId = 1)
        {
            var user = new User { UserId = userId };
            _context.Users.Add(user);

            var type = new TypeEntity
            {
                TypeId = typeId,
                Name = "Test Type",
                Archived = false
            };
            _context.Types.Add(type);
            await _context.SaveChangesAsync();
            return (user, type);
        }

        private async Task<AnonymousComplaint> SeedComplaintAsync(
            int id, string description = "Test Complaint",
            int? createdBy = null, string state = "SUBMITTED",
            bool archived = false, DateTime? createdAt = null)
        {
            var complaint = new AnonymousComplaint
            {
                AnonymousComplaintId = id,
                Description = description,
                CreatedBy = createdBy,
                State = state,
                Archived = archived,
                CreatedAt = createdAt ?? DateTime.Now
            };
            _context.AnonymousComplaints.Add(complaint);
            await _context.SaveChangesAsync();
            return complaint;
        }

        // ================================================================
        // GET ALL - GET /api/AnonymousComplaints
        // ================================================================
        [Fact]
        public async Task GetAnonymousComplaints_ReturnsOk_WithEmptyList()
        {
            var response = await _client.GetAsync("/api/AnonymousComplaints");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAnonymousComplaints_ReturnsOk_WithData()
        {
            await SeedComplaintAsync(100, "Integration Test Complaint 1");
            await SeedComplaintAsync(101, "Integration Test Complaint 2");

            var response = await _client.GetAsync("/api/AnonymousComplaints");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Integration Test Complaint 1");
            content.Should().Contain("Integration Test Complaint 2");
        }

        // ================================================================
        // GET BY USER - GET /api/AnonymousComplaints/user/{userId}
        // ================================================================
        [Fact]
        public async Task GetAnonymousComplaintsByUser_ReturnsOk()
        {
            var (user, _) = await SeedUserAndTypeAsync(userId: 10);
            await SeedComplaintAsync(200, "User Complaint", createdBy: 10);

            var response = await _client.GetAsync("/api/AnonymousComplaints/user/10");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("User Complaint");
        }

        [Fact]
        public async Task GetAnonymousComplaintsByUser_NewUser_ReturnsOk()
        {
            // User not in DB → FakeEnsureService creates them automatically
            var response = await _client.GetAsync("/api/AnonymousComplaints/user/999");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ================================================================
        // CREATE - POST /api/AnonymousComplaints (multipart/form-data)
        // ================================================================
        [Fact]
        public async Task PostAnonymousComplaint_ValidData_ReturnsOk()
        {
            var (user, type) = await SeedUserAndTypeAsync(userId: 1, typeId: 1);

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent("New complaint description"), "Description");
            formData.Add(new StringContent("1"), "CreatedBy");
            formData.Add(new StringContent("1"), "TypeID");

            var response = await _client.PostAsync("/api/AnonymousComplaints", formData);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task PostAnonymousComplaint_MissingCreatedBy_ReturnsBadRequest()
        {
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent("Complaint without user"), "Description");
            formData.Add(new StringContent("1"), "TypeID");

            var response = await _client.PostAsync("/api/AnonymousComplaints", formData);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PostAnonymousComplaint_InvalidTypeID_ReturnsBadRequest()
        {
            await SeedUserAndTypeAsync(userId: 1, typeId: 1);

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent("Complaint with bad type"), "Description");
            formData.Add(new StringContent("1"), "CreatedBy");
            formData.Add(new StringContent("9999"), "TypeID"); // non-existent

            var response = await _client.PostAsync("/api/AnonymousComplaints", formData);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PostAnonymousComplaint_WithFileUpload_ReturnsOk()
        {
            var (user, type) = await SeedUserAndTypeAsync(userId: 1, typeId: 1);

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent("Complaint with file"), "Description");
            formData.Add(new StringContent("1"), "CreatedBy");
            formData.Add(new StringContent("1"), "TypeID");

            // Add a fake file
            var fileContent = new ByteArrayContent(new byte[] { 0x01, 0x02, 0x03 });
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            formData.Add(fileContent, "File", "testfile.txt");

            var response = await _client.PostAsync("/api/AnonymousComplaints", formData);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ================================================================
        // UPDATE - PUT /api/AnonymousComplaints/{id}
        // ================================================================
        [Fact]
        public async Task PutAnonymousComplaint_ValidData_ReturnsOk()
        {
            var (user, type) = await SeedUserAndTypeAsync(userId: 1, typeId: 1);
            await SeedComplaintAsync(300, "Old Description", createdBy: 1);

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent("300"), "Id");
            formData.Add(new StringContent("Updated Description"), "Description");
            formData.Add(new StringContent("1"), "CreatedBy");
            formData.Add(new StringContent("1"), "TypeID");

            var response = await _client.PutAsync("/api/AnonymousComplaints/300", formData);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Updated Description");
        }

        [Fact]
        public async Task PutAnonymousComplaint_IdMismatch_ReturnsBadRequest()
        {
            var (user, type) = await SeedUserAndTypeAsync(userId: 1, typeId: 1);
            await SeedComplaintAsync(301, "Original", createdBy: 1);

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent("999"), "Id"); // mismatch with URL id=301
            formData.Add(new StringContent("Updated"), "Description");
            formData.Add(new StringContent("1"), "CreatedBy");

            var response = await _client.PutAsync("/api/AnonymousComplaints/301", formData);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PutAnonymousComplaint_NotFound_ReturnsNotFound()
        {
            await SeedUserAndTypeAsync(userId: 1, typeId: 1);

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent("9999"), "Id");
            formData.Add(new StringContent("Updated"), "Description");
            formData.Add(new StringContent("1"), "CreatedBy");

            var response = await _client.PutAsync("/api/AnonymousComplaints/9999", formData);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task PutAnonymousComplaint_ArchivedComplaint_ReturnsNotFound()
        {
            await SeedUserAndTypeAsync(userId: 1, typeId: 1);
            await SeedComplaintAsync(302, "Archived Complaint", createdBy: 1, archived: true);

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent("302"), "Id");
            formData.Add(new StringContent("Updated"), "Description");
            formData.Add(new StringContent("1"), "CreatedBy");

            var response = await _client.PutAsync("/api/AnonymousComplaints/302", formData);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // ARCHIVE - DELETE /api/AnonymousComplaints/{id}
        // ================================================================
        [Fact]
        public async Task ArchiveAnonymousComplaint_ExistingComplaint_ReturnsNoContent()
        {
            await SeedComplaintAsync(400, "To Archive");

            var response = await _client.DeleteAsync("/api/AnonymousComplaints/400");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify it is archived in DB (reload to get server-side changes)
            var complaint = await _context.AnonymousComplaints.FindAsync(400);
            await _context.Entry(complaint!).ReloadAsync();
            complaint.Archived.Should().BeTrue();
        }

        [Fact]
        public async Task ArchiveAnonymousComplaint_NotFound_ReturnsNotFound()
        {
            var response = await _client.DeleteAsync("/api/AnonymousComplaints/9999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // RESTORE - PUT /api/AnonymousComplaints/restore/{id}
        // ================================================================
        [Fact]
        public async Task RestoreAnonymousComplaint_ArchivedComplaint_ReturnsNoContent()
        {
            await SeedComplaintAsync(500, "Archived Complaint", archived: true);

            var response = await _client.PutAsync("/api/AnonymousComplaints/restore/500", null);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify it is restored (reload to get server-side changes)
            var complaint = await _context.AnonymousComplaints.FindAsync(500);
            await _context.Entry(complaint!).ReloadAsync();
            complaint.Archived.Should().BeFalse();
        }

        [Fact]
        public async Task RestoreAnonymousComplaint_NotArchived_ReturnsNotFound()
        {
            await SeedComplaintAsync(501, "Active Complaint", archived: false);

            var response = await _client.PutAsync("/api/AnonymousComplaints/restore/501", null);

            // Controller checks (entity.Archived != true) → returns NotFound
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task RestoreAnonymousComplaint_NotFound_ReturnsNotFound()
        {
            var response = await _client.PutAsync("/api/AnonymousComplaints/restore/9999", null);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // MERGE - POST /api/AnonymousComplaints/merge
        // ================================================================
        [Fact]
        public async Task MergeComplaints_ValidIds_ReturnsOk()
        {
            await SeedComplaintAsync(600, "Oldest Complaint", createdAt: DateTime.Now.AddDays(-5));
            await SeedComplaintAsync(601, "Newer Complaint", createdAt: DateTime.Now.AddDays(-1));

            var ids = new List<int> { 600, 601 };
            var response = await _client.PostAsJsonAsync("/api/AnonymousComplaints/merge", ids);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify the newer complaint is fused with the oldest (reload to get server-side changes)
            var newerComplaint = await _context.AnonymousComplaints.FindAsync(601);
            await _context.Entry(newerComplaint!).ReloadAsync();
            newerComplaint.FusionWithId.Should().Be(600);
        }

        [Fact]
        public async Task MergeComplaints_ThreeComplaints_ReturnsOk()
        {
            await SeedComplaintAsync(610, "Oldest", createdAt: DateTime.Now.AddDays(-10));
            await SeedComplaintAsync(611, "Middle", createdAt: DateTime.Now.AddDays(-5));
            await SeedComplaintAsync(612, "Newest", createdAt: DateTime.Now);

            var ids = new List<int> { 610, 611, 612 };
            var response = await _client.PostAsJsonAsync("/api/AnonymousComplaints/merge", ids);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // All should reference the oldest one (reload to get server-side changes)
            var middle = await _context.AnonymousComplaints.FindAsync(611);
            await _context.Entry(middle!).ReloadAsync();
            var newest = await _context.AnonymousComplaints.FindAsync(612);
            await _context.Entry(newest!).ReloadAsync();
            middle.FusionWithId.Should().Be(610);
            newest.FusionWithId.Should().Be(610);
        }

        [Fact]
        public async Task MergeComplaints_LessThanTwo_ReturnsBadRequest()
        {
            await SeedComplaintAsync(620, "Only One");

            var ids = new List<int> { 620 };
            var response = await _client.PostAsJsonAsync("/api/AnonymousComplaints/merge", ids);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task MergeComplaints_EmptyList_ReturnsBadRequest()
        {
            var ids = new List<int>();
            var response = await _client.PostAsJsonAsync("/api/AnonymousComplaints/merge", ids);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task MergeComplaints_NonExistentIds_ReturnsNotFound()
        {
            var ids = new List<int> { 9990, 9991 };
            var response = await _client.PostAsJsonAsync("/api/AnonymousComplaints/merge", ids);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // CHANGE STATE - PUT /api/AnonymousComplaints/ChangeState/{id}
        // ================================================================
        [Fact]
        public async Task ChangeState_SubmittedComplaint_ReturnsOk()
        {
            await SeedComplaintAsync(700, "Submitted Complaint", state: "SUBMITTED");

            var response = await _client.PutAsync("/api/AnonymousComplaints/ChangeState/700", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify state changed to IN PROGRESS (reload to get server-side changes)
            var complaint = await _context.AnonymousComplaints.FindAsync(700);
            await _context.Entry(complaint!).ReloadAsync();
            complaint.State.Should().Be("IN PROGRESS");
        }

        [Fact]
        public async Task ChangeState_NonSubmittedComplaint_ReturnsOk_NoStateChange()
        {
            await SeedComplaintAsync(701, "In Progress Complaint", state: "IN PROGRESS");

            var response = await _client.PutAsync("/api/AnonymousComplaints/ChangeState/701", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // State should remain unchanged (only SUBMITTED → IN PROGRESS)
            var complaint = await _context.AnonymousComplaints.FindAsync(701);
            complaint!.State.Should().Be("IN PROGRESS");
        }

        [Fact]
        public async Task ChangeState_ArchivedComplaint_ReturnsNotFound()
        {
            await SeedComplaintAsync(702, "Archived", state: "SUBMITTED", archived: true);

            var response = await _client.PutAsync("/api/AnonymousComplaints/ChangeState/702", null);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ChangeState_NotFound_ReturnsNotFound()
        {
            var response = await _client.PutAsync("/api/AnonymousComplaints/ChangeState/9999", null);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // END-TO-END: Full workflow
        // ================================================================
        [Fact]
        public async Task FullWorkflow_Create_ChangeState_Archive()
        {
            // 1. Seed user and type (userId=1 matches the TestAuthHandler default)
            var (user, type) = await SeedUserAndTypeAsync(userId: 1, typeId: 50);

            // 2. Create complaint (userId from token = 1)
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent("Full workflow complaint"), "Description");
            formData.Add(new StringContent("50"), "TypeID");

            var createResponse = await _client.PostAsync("/api/AnonymousComplaints", formData);
            createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Get the created complaint from DB
            var created = await _context.AnonymousComplaints
                .FirstOrDefaultAsync(c => c.Description == "Full workflow complaint");
            created.Should().NotBeNull();
            created!.State.Should().Be("DÉPOSÉ");

            // 3. Change state to IN PROGRESS
            var stateResponse = await _client.PutAsync(
                $"/api/AnonymousComplaints/ChangeState/{created.AnonymousComplaintId}", null);
            stateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            await _context.Entry(created).ReloadAsync();
            created.State.Should().Be("IN PROGRESS");

            // 4. Add solution via SolutionsController (resolves complaint)
            var solutionDto = new SendResponseRequest
            {
                Content = "Problem resolved - solution applied successfully",
                AnonymousComplaintID = created.AnonymousComplaintId,
                Archived = false,
                CreatedAt = DateTime.Now
            };
            var addResp = await _client.PostAsJsonAsync("/api/Solutions", solutionDto);
            addResp.StatusCode.Should().Be(HttpStatusCode.Created);

            await _context.Entry(created).ReloadAsync();
            created.State.Should().Be("RESOLVED");

            // 5. Archive the complaint
            var archiveResp = await _client.DeleteAsync(
                $"/api/AnonymousComplaints/{created.AnonymousComplaintId}");
            archiveResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

            await _context.Entry(created).ReloadAsync();
            created.Archived.Should().BeTrue();
        }
    }
}
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
    public class AttachmentsControllerTests
        : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private AnonymousComplaintsV002Context _context = null!;
        private IServiceScope _scope = null!;

        public AttachmentsControllerTests(CustomWebApplicationFactory factory)
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

            // Clean data before each test
            _context.Attachments.RemoveRange(_context.Attachments);
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
        // Helper
        // ================================================================
        private async Task<Attachment> SeedAttachmentAsync(
            int id, string fileName = "test.pdf",
            bool archived = false, int? complaintId = null)
        {
            var attachment = new Attachment
            {
                AttachmentId = id,
                FileName = fileName,
                FilePath = $"/uploads/{fileName}",
                FileType = "application/pdf",
                Archived = archived,
                CreatedAt = DateTime.Now,
                AnonymousComplaintId = complaintId
            };
            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();
            return attachment;
        }

        private async Task<AnonymousComplaint> SeedComplaintWithAttachmentsAsync(int complaintId)
        {
            var complaint = new AnonymousComplaint
            {
                AnonymousComplaintId = complaintId,
                Description = "Complaint with files",
                State = "SUBMITTED",
                Archived = false
            };
            _context.AnonymousComplaints.Add(complaint);

            _context.Attachments.AddRange(
                new Attachment
                {
                    AttachmentId = complaintId * 100 + 1,
                    FileName = "file1.pdf",
                    FilePath = "/uploads/file1.pdf",
                    FileType = "application/pdf",
                    AnonymousComplaintId = complaintId,
                    Archived = false,
                    CreatedAt = DateTime.Now
                },
                new Attachment
                {
                    AttachmentId = complaintId * 100 + 2,
                    FileName = "file2.png",
                    FilePath = "/uploads/file2.png",
                    FileType = "image/png",
                    AnonymousComplaintId = complaintId,
                    Archived = false,
                    CreatedAt = DateTime.Now
                });
            await _context.SaveChangesAsync();
            return complaint;
        }

        // ================================================================
        // GET ALL - GET /api/Attachments
        // ================================================================
        [Fact]
        public async Task GetAttachments_ReturnsOk_WithEmptyList()
        {
            var response = await _client.GetAsync("/api/Attachments");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAttachments_ReturnsOnlyNonArchived()
        {
            await SeedAttachmentAsync(100, "active.pdf", archived: false);
            await SeedAttachmentAsync(101, "archived.pdf", archived: true);

            var response = await _client.GetAsync("/api/Attachments");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("active.pdf");
            content.Should().NotContain("archived.pdf");
        }

        // ================================================================
        // GET BY ID - GET /api/Attachments/{id}
        // ================================================================
        [Fact]
        public async Task GetAttachment_ExistingId_ReturnsOk()
        {
            await SeedAttachmentAsync(200, "document.pdf");

            var response = await _client.GetAsync("/api/Attachments/200");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("document.pdf");
        }

        [Fact]
        public async Task GetAttachment_NonExistentId_ReturnsNotFound()
        {
            var response = await _client.GetAsync("/api/Attachments/9999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetAttachment_ArchivedId_ReturnsNotFound()
        {
            await SeedAttachmentAsync(201, "archived.pdf", archived: true);

            var response = await _client.GetAsync("/api/Attachments/201");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // CREATE - POST /api/Attachments
        // ================================================================
        [Fact]
        public async Task PostAttachment_ValidData_ReturnsCreated()
        {
            var dto = new AttachmentResponse
            {
                FileName = "new_file.pdf",
                FilePath = "/uploads/new_file.pdf",
                FileType = "application/pdf",
                Archived = false,
                CreatedAt = DateTime.Now
            };

            var response = await _client.PostAsJsonAsync("/api/Attachments", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("new_file.pdf");
        }

        [Fact]
        public async Task PostAttachment_WithComplaintId_ReturnsCreated()
        {
            // Seed a complaint first
            _context.AnonymousComplaints.Add(new AnonymousComplaint
            {
                AnonymousComplaintId = 300,
                Description = "Parent complaint",
                State = "SUBMITTED",
                Archived = false
            });
            await _context.SaveChangesAsync();

            var dto = new AttachmentResponse
            {
                FileName = "complaint_file.pdf",
                FilePath = "/uploads/complaint_file.pdf",
                FileType = "application/pdf",
                AnonymousComplaintID = 300,
                Archived = false
            };

            var response = await _client.PostAsJsonAsync("/api/Attachments", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // ================================================================
        // UPLOAD - POST /api/Attachments/upload
        // ================================================================
        [Fact]
        public async Task UploadAttachment_ValidFile_ReturnsCreated()
        {
            var formData = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(new byte[] { 0x25, 0x50, 0x44, 0x46 }); // PDF magic bytes
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            formData.Add(fileContent, "file", "uploaded_test.pdf");

            var response = await _client.PostAsync("/api/Attachments/upload", formData);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("uploaded_test.pdf");
        }

        [Fact]
        public async Task UploadAttachment_NoFile_ReturnsBadRequest()
        {
            var formData = new MultipartFormDataContent();
            // Empty content, no file
            formData.Add(new StringContent(""), "file");

            var response = await _client.PostAsync("/api/Attachments/upload", formData);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ================================================================
        // UPDATE - PUT /api/Attachments/{id}
        // ================================================================
        [Fact]
        public async Task PutAttachment_ValidData_ReturnsNoContent()
        {
            await SeedAttachmentAsync(400, "original.pdf");

            var dto = new AttachmentResponse
            {
                Id = 400,
                FileName = "renamed.pdf",
                FilePath = "/uploads/renamed.pdf",
                FileType = "application/pdf",
                Archived = false
            };

            var response = await _client.PutAsJsonAsync("/api/Attachments/400", dto);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify update in DB
            var attachment = await _context.Attachments.FindAsync(400);
            await _context.Entry(attachment!).ReloadAsync();
            attachment.FileName.Should().Be("renamed.pdf");
        }

        [Fact]
        public async Task PutAttachment_IdMismatch_ReturnsBadRequest()
        {
            await SeedAttachmentAsync(401, "file.pdf");

            var dto = new AttachmentResponse
            {
                Id = 999, // Mismatch with URL id=401
                FileName = "renamed.pdf",
                FilePath = "/uploads/renamed.pdf"
            };

            var response = await _client.PutAsJsonAsync("/api/Attachments/401", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PutAttachment_NotFound_ReturnsNotFound()
        {
            var dto = new AttachmentResponse
            {
                Id = 9999,
                FileName = "ghost.pdf",
                FilePath = "/uploads/ghost.pdf"
            };

            var response = await _client.PutAsJsonAsync("/api/Attachments/9999", dto);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task PutAttachment_ArchivedAttachment_ReturnsNotFound()
        {
            await SeedAttachmentAsync(402, "archived.pdf", archived: true);

            var dto = new AttachmentResponse
            {
                Id = 402,
                FileName = "updated.pdf",
                FilePath = "/uploads/updated.pdf"
            };

            var response = await _client.PutAsJsonAsync("/api/Attachments/402", dto);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // SOFT DELETE - DELETE /api/Attachments/{id}
        // ================================================================
        [Fact]
        public async Task SoftDeleteAttachment_Existing_ReturnsNoContent()
        {
            await SeedAttachmentAsync(500, "to_archive.pdf");

            var response = await _client.DeleteAsync("/api/Attachments/500");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify archived in DB
            var attachment = await _context.Attachments.FindAsync(500);
            await _context.Entry(attachment!).ReloadAsync();
            attachment.Archived.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAttachment_NotFound_ReturnsNotFound()
        {
            var response = await _client.DeleteAsync("/api/Attachments/9999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // RESTORE - PUT /api/Attachments/restore/{id}
        // ================================================================
        [Fact]
        public async Task RestoreAttachment_Archived_ReturnsNoContent()
        {
            await SeedAttachmentAsync(600, "archived.pdf", archived: true);

            var response = await _client.PutAsync("/api/Attachments/restore/600", null);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify restored
            var attachment = await _context.Attachments.FindAsync(600);
            await _context.Entry(attachment!).ReloadAsync();
            attachment.Archived.Should().BeFalse();
        }

        [Fact]
        public async Task RestoreAttachment_NotFound_ReturnsNotFound()
        {
            var response = await _client.PutAsync("/api/Attachments/restore/9999", null);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // HARD DELETE - DELETE /api/Attachments/hard-delete/{id}
        // ================================================================
        [Fact]
        public async Task HardDeleteAttachment_Existing_ReturnsNoContent()
        {
            await SeedAttachmentAsync(700, "to_delete.pdf");

            var response = await _client.DeleteAsync("/api/Attachments/hard-delete/700");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify removed from DB (detach local cache and re-query)
            var local = _context.Attachments.Local.FirstOrDefault(a => a.AttachmentId == 700);
            if (local != null)
                _context.Entry(local).State = EntityState.Detached;

            var attachment = await _context.Attachments.FindAsync(700);
            attachment.Should().BeNull();
        }

        [Fact]
        public async Task HardDeleteAttachment_NotFound_ReturnsNotFound()
        {
            var response = await _client.DeleteAsync("/api/Attachments/hard-delete/9999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // DOWNLOAD ALL - GET /api/Attachments/download-all/{complaintId}
        // ================================================================
        [Fact]
        public async Task DownloadAllFiles_NoComplaint_ReturnsNotFound()
        {
            var response = await _client.GetAsync("/api/Attachments/download-all/9999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DownloadAllFiles_ComplaintWithNoAttachments_ReturnsNotFound()
        {
            _context.AnonymousComplaints.Add(new AnonymousComplaint
            {
                AnonymousComplaintId = 800,
                Description = "No files complaint",
                State = "SUBMITTED",
                Archived = false
            });
            await _context.SaveChangesAsync();

            var response = await _client.GetAsync("/api/Attachments/download-all/800");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ================================================================
        // END-TO-END: Full attachment lifecycle
        // ================================================================
        [Fact]
        public async Task FullLifecycle_Create_Update_Archive_Restore_HardDelete()
        {
            // 1. Create attachment
            var createDto = new AttachmentResponse
            {
                FileName = "lifecycle.pdf",
                FilePath = "/uploads/lifecycle.pdf",
                FileType = "application/pdf",
                Archived = false,
                CreatedAt = DateTime.Now
            };

            var createResponse = await _client.PostAsJsonAsync("/api/Attachments", createDto);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var created = await createResponse.Content.ReadFromJsonAsync<AttachmentResponse>();
            created.Should().NotBeNull();
            var id = created!.Id;

            // 2. Get it
            var getResponse = await _client.GetAsync($"/api/Attachments/{id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // 3. Update it
            var updateDto = new AttachmentResponse
            {
                Id = id,
                FileName = "lifecycle_renamed.pdf",
                FilePath = "/uploads/lifecycle_renamed.pdf",
                FileType = "application/pdf",
                Archived = false
            };
            var updateResponse = await _client.PutAsJsonAsync($"/api/Attachments/{id}", updateDto);
            updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 4. Soft delete (archive)
            var archiveResponse = await _client.DeleteAsync($"/api/Attachments/{id}");
            archiveResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 5. Verify archived (GET returns NotFound)
            var archivedGet = await _client.GetAsync($"/api/Attachments/{id}");
            archivedGet.StatusCode.Should().Be(HttpStatusCode.NotFound);

            // 6. Restore
            var restoreResponse = await _client.PutAsync($"/api/Attachments/restore/{id}", null);
            restoreResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 7. Verify restored (GET returns OK)
            var restoredGet = await _client.GetAsync($"/api/Attachments/{id}");
            restoredGet.StatusCode.Should().Be(HttpStatusCode.OK);

            // 8. Hard delete
            var hardDeleteResponse = await _client.DeleteAsync($"/api/Attachments/hard-delete/{id}");
            hardDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 9. Verify gone
            var goneGet = await _client.GetAsync($"/api/Attachments/{id}");
            goneGet.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}

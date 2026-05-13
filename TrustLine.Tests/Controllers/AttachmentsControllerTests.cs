using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Http;

using AnonymousComplaintsAPI.Controllers;
using AnonymousComplaintsAPI.Data;
using AnonymousComplaintsAPI.Models.Entities;
using AnonymousComplaintsAPI.DTOs.Responses;

namespace TrustLine.Tests.Controllers
{
    public class AttachmentsControllerTests
    {
        // =========================
        // DB IN MEMORY
        // =========================
        private AnonymousComplaintsV002Context CreateDb()
        {
            var options = new DbContextOptionsBuilder<AnonymousComplaintsV002Context>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AnonymousComplaintsV002Context(options);
        }

        private AttachmentsController CreateController(AnonymousComplaintsV002Context context)
        {
            return new AttachmentsController(context);
        }

        // =====================================================
        // GET ALL
        // =====================================================
        [Fact]
        public async Task GetAttachments_ShouldReturnList()
        {
            var context = CreateDb();

            context.Attachments.Add(new Attachment
            {
                AttachmentId = 1,
                FileName = "file.pdf",
                FilePath = "/uploads/file.pdf",
                Archived = false
            });

            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var result = await controller.GetAttachments();

            var list = Assert.IsType<List<AttachmentResponse>>(result.Value);

            Assert.Single(list);
        }

        // =====================================================
        // GET BY ID
        // =====================================================
        [Fact]
        public async Task GetAttachment_ShouldReturnAttachment()
        {
            var context = CreateDb();

            context.Attachments.Add(new Attachment
            {
                AttachmentId = 1,
                FileName = "file.pdf",
                FilePath = "/uploads/file.pdf",
                Archived = false
            });

            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var result = await controller.GetAttachment(1);

            Assert.NotNull(result.Value);
            Assert.Equal("file.pdf", result.Value.FileName);
        }

        [Fact]
        public async Task GetAttachment_ShouldReturnNotFound()
        {
            var context = CreateDb();
            var controller = CreateController(context);

            var result = await controller.GetAttachment(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        // =====================================================
        // CREATE
        // =====================================================
        [Fact]
        public async Task PostAttachment_ShouldCreate()
        {
            var context = CreateDb();
            var controller = CreateController(context);

            var dto = new AttachmentResponse
            {
                FileName = "file.pdf",
                FilePath = "path",
                FileType = "application/pdf"
            };

            var result = await controller.PostAttachment(dto);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);

            Assert.Equal(1, context.Attachments.Count());
        }

        // =====================================================
        // UPDATE
        // =====================================================
        [Fact]
        public async Task PutAttachment_ShouldUpdate()
        {
            var context = CreateDb();

            context.Attachments.Add(new Attachment
            {
                AttachmentId = 1,
                FileName = "old.pdf",
                FilePath = "/uploads/old.pdf",
                Archived = false
            });

            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var dto = new AttachmentResponse
            {
                Id = 1,
                FileName = "new.pdf"
            };

            var result = await controller.PutAttachment(1, dto);

            Assert.IsType<NoContentResult>(result);

            var updated = await context.Attachments.FindAsync(1);
            Assert.Equal("new.pdf", updated!.FileName);
        }

        // =====================================================
        // DELETE (SOFT)
        // =====================================================
        [Fact]
        public async Task SoftDelete_ShouldArchive()
        {
            var context = CreateDb();

            context.Attachments.Add(new Attachment
            {
                AttachmentId = 1,
                FilePath = "/uploads/test",
                Archived = false
            });

            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var result = await controller.SoftDeleteAttachment(1);

            Assert.IsType<NoContentResult>(result);

            var entity = await context.Attachments.FindAsync(1);
            Assert.True(entity!.Archived);
        }

        // =====================================================
        // RESTORE
        // =====================================================
        [Fact]
        public async Task Restore_ShouldUnarchive()
        {
            var context = CreateDb();

            context.Attachments.Add(new Attachment
            {
                AttachmentId = 1,
                FilePath = "/uploads/test",
                Archived = true
            });

            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var result = await controller.RestoreAttachment(1);

            Assert.IsType<NoContentResult>(result);

            var entity = await context.Attachments.FindAsync(1);
            Assert.False(entity!.Archived);
        }

        // =====================================================
        // HARD DELETE
        // =====================================================
        [Fact]
        public async Task HardDelete_ShouldRemove()
        {
            var context = CreateDb();

            context.Attachments.Add(new Attachment
            {
                AttachmentId = 1,
                FilePath = "/uploads/test"
            });

            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var result = await controller.HardDeleteAttachment(1);

            Assert.IsType<NoContentResult>(result);
            Assert.Empty(context.Attachments);
        }

        // =====================================================
        // UPLOAD
        // =====================================================
        [Fact]
        public async Task UploadAttachment_ShouldCreateFile()
        {
            var context = CreateDb();
            var controller = CreateController(context);

            var fileName = "test.txt";
            var content = "Hello World";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

            IFormFile file = new FormFile(stream, 0, stream.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };

            var result = await controller.UploadAttachment(file);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);

            Assert.Equal(1, context.Attachments.Count());
        }

        // =====================================================
        // DOWNLOAD ZIP
        // =====================================================
        [Fact]
        public async Task DownloadAllFiles_ShouldReturnNotFound()
        {
            var context = CreateDb();
            var controller = CreateController(context);

            var result = await controller.DownloadAllFiles(1);

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
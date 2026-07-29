using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AnonymousComplaintsAPI.Services.Implementations;
using AnonymousComplaintsAPI.Services.Interfaces;
using AnonymousComplaintsAPI.Repositories.Interfaces;
using AnonymousComplaintsAPI.DTOs.Responses;
using AnonymousComplaintsAPI.Models.Entities;

namespace TrustLine.Tests.Services
{
    public class AttachmentServiceTests
    {
        private readonly Mock<IAttachmentRepository> _attachmentRepo = new();
        private readonly Mock<IAnonymousComplaintRepository> _complaintRepo = new();
        private readonly Mock<IFileService> _fileService = new();
        private readonly Mock<ILogger<AttachmentService>> _logger = new();

        private AttachmentService CreateService()
        {
            return new AttachmentService(
                _attachmentRepo.Object,
                _complaintRepo.Object,
                _fileService.Object,
                _logger.Object
            );
        }

        // =========================
        // GET BY ID
        // =========================

        [Fact]
        public async Task GetAttachment_ShouldReturnAttachment()
        {
            var attachment = new Attachment
            {
                AttachmentId = 1,
                FileName = "file.pdf"
            };

            _attachmentRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(attachment);

            var service = CreateService();

            var result = await service.GetAttachmentAsync(1);

            Assert.NotNull(result);
            Assert.Equal("file.pdf", result!.FileName);
        }

        [Fact]
        public async Task GetAttachment_ShouldReturnNull_WhenNotFound()
        {
            _attachmentRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Attachment?)null);

            var service = CreateService();

            var result = await service.GetAttachmentAsync(99);

            Assert.Null(result);
        }

        // =========================
        // UPLOAD
        // =========================

        [Fact]
        public async Task UploadAttachment_ShouldCreateAttachment()
        {
            var fakeFile = new Mock<IFormFile>();
            fakeFile.Setup(f => f.FileName).Returns("test.pdf");

            _fileService.Setup(x => x.SaveFileAsync(It.IsAny<IFormFile>(), "Uploads"))
                .ReturnsAsync("path/test.pdf");

            _attachmentRepo.Setup(x => x.CreateAsync(It.IsAny<Attachment>()))
                .ReturnsAsync((Attachment a) =>
                {
                    a.AttachmentId = 1;
                    return a;
                });

            var service = CreateService();

            var result = await service.UploadAttachmentAsync(fakeFile.Object, 1);

            Assert.NotNull(result);
            Assert.Equal("test.pdf", result.FileName);
        }

        [Fact]
        public async Task UploadAttachment_ShouldWork_WhenComplaintIdIsNull()
        {
            var fakeFile = new Mock<IFormFile>();
            fakeFile.Setup(f => f.FileName).Returns("test.pdf");

            _fileService.Setup(x => x.SaveFileAsync(It.IsAny<IFormFile>(), "Uploads"))
                .ReturnsAsync("path/test.pdf");

            _attachmentRepo.Setup(x => x.CreateAsync(It.IsAny<Attachment>()))
                .ReturnsAsync((Attachment a) =>
                {
                    a.AttachmentId = 1;
                    return a;
                });

            var service = CreateService();

            var result = await service.UploadAttachmentAsync(fakeFile.Object, null);

            Assert.NotNull(result);
        }

        // =========================
        // DELETE
        // =========================

        [Fact]
        public async Task DeleteAttachment_ShouldDeleteFileAndEntity()
        {
            var attachment = new Attachment
            {
                AttachmentId = 1,
                FilePath = "file.pdf"
            };

            _attachmentRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(attachment);

            _fileService.Setup(x => x.DeleteFileAsync("file.pdf"))
                .Returns(Task.CompletedTask);

            _attachmentRepo.Setup(x => x.DeleteAsync(1))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            await service.DeleteAttachmentAsync(1);

            _fileService.Verify(x => x.DeleteFileAsync("file.pdf"), Times.Once);
            _attachmentRepo.Verify(x => x.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteAttachment_ShouldNotCallFileService_WhenAttachmentNotFound()
        {
            _attachmentRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((Attachment?)null);

            var service = CreateService();

            await service.DeleteAttachmentAsync(1);

            _fileService.Verify(x => x.DeleteFileAsync(It.IsAny<string>()), Times.Never);
            _attachmentRepo.Verify(x => x.DeleteAsync(1), Times.Once);
        }

        // =========================
        // ZIP GENERATION
        // =========================

        [Fact]
        public async Task GenerateZip_ShouldCallFileService()
        {
            var attachments = new List<Attachment>
            {
                new Attachment { FilePath = "file1.pdf" },
                new Attachment { FilePath = "file2.pdf" }
            };

            _attachmentRepo.Setup(x => x.GetByComplaintIdAsync(1))
                .ReturnsAsync(attachments);

            _fileService.Setup(x => x.FileExists(It.IsAny<string>()))
                .Returns(true);

            _fileService.Setup(x => x.CreateZipFromFilesAsync(It.IsAny<List<string>>(), It.IsAny<string>()))
                .ReturnsAsync(new MemoryStream());

            var service = CreateService();

            var result = await service.GenerateComplaintAttachmentsZipAsync(1);

            Assert.NotNull(result);

            _fileService.Verify(
                x => x.CreateZipFromFilesAsync(It.IsAny<List<string>>(), It.IsAny<string>()),
                Times.Once);
        }
    }
}
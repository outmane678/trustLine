using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

using AnonymousComplaintsAPI.Controllers;
using AnonymousComplaintsAPI.Services.Interfaces;
using AnonymousComplaintsAPI.DTOs.Responses;

namespace TrustLine.Tests.Controllers
{
    public class AttachmentsControllerTests
    {
        private readonly Mock<IAttachmentService> _serviceMock = new();

        private AttachmentsController CreateController() =>
            new AttachmentsController(_serviceMock.Object);

        // =====================================================
        // GET ALL
        // =====================================================
        [Fact]
        public async Task GetAttachments_ShouldReturnOk()
        {
            _serviceMock.Setup(x => x.GetAllAttachmentsAsync())
                .ReturnsAsync(new List<AttachmentResponse>
                {
                    new AttachmentResponse { Id = 1, FileName = "file.pdf" }
                });

            var result = await CreateController().GetAttachments();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<AttachmentResponse>>(ok.Value);
            Assert.Single(list);
        }

        // =====================================================
        // GET BY ID
        // =====================================================
        [Fact]
        public async Task GetAttachment_ShouldReturnOk()
        {
            _serviceMock.Setup(x => x.GetAttachmentAsync(1))
                .ReturnsAsync(new AttachmentResponse { Id = 1, FileName = "file.pdf" });

            var result = await CreateController().GetAttachment(1);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task GetAttachment_NotFound_ShouldReturnNotFound()
        {
            _serviceMock.Setup(x => x.GetAttachmentAsync(999))
                .ReturnsAsync((AttachmentResponse?)null);

            var result = await CreateController().GetAttachment(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        // =====================================================
        // CREATE
        // =====================================================
        [Fact]
        public async Task PostAttachment_ShouldReturnCreated()
        {
            var dto = new AttachmentResponse { FileName = "file.pdf", FilePath = "path", FileType = "pdf" };
            var created = new AttachmentResponse { Id = 1, FileName = "file.pdf" };

            _serviceMock.Setup(x => x.CreateAttachmentAsync(dto))
                .ReturnsAsync(created);

            var result = await CreateController().PostAttachment(dto);

            Assert.IsType<CreatedAtActionResult>(result.Result);
        }

        // =====================================================
        // UPDATE
        // =====================================================
        [Fact]
        public async Task PutAttachment_ShouldReturnNoContent()
        {
            var dto = new AttachmentResponse { Id = 1, FileName = "new.pdf" };

            _serviceMock.Setup(x => x.UpdateAttachmentAsync(1, dto))
                .ReturnsAsync(new AttachmentResponse { Id = 1, FileName = "new.pdf" });

            var result = await CreateController().PutAttachment(1, dto);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task PutAttachment_IdMismatch_ShouldReturnBadRequest()
        {
            var dto = new AttachmentResponse { Id = 2 };

            var result = await CreateController().PutAttachment(1, dto);

            Assert.IsType<BadRequestResult>(result);
        }

        // =====================================================
        // SOFT DELETE
        // =====================================================
        [Fact]
        public async Task SoftDeleteAttachment_ShouldReturnNoContent()
        {
            _serviceMock.Setup(x => x.ArchiveAttachmentAsync(1))
                .Returns(Task.CompletedTask);

            var result = await CreateController().SoftDeleteAttachment(1);

            Assert.IsType<NoContentResult>(result);
        }

        // =====================================================
        // RESTORE
        // =====================================================
        [Fact]
        public async Task RestoreAttachment_ShouldReturnNoContent()
        {
            _serviceMock.Setup(x => x.RestoreAttachmentAsync(1))
                .Returns(Task.CompletedTask);

            var result = await CreateController().RestoreAttachment(1);

            Assert.IsType<NoContentResult>(result);
        }

        // =====================================================
        // HARD DELETE
        // =====================================================
        [Fact]
        public async Task HardDeleteAttachment_ShouldReturnNoContent()
        {
            _serviceMock.Setup(x => x.DeleteAttachmentAsync(1))
                .Returns(Task.CompletedTask);

            var result = await CreateController().HardDeleteAttachment(1);

            Assert.IsType<NoContentResult>(result);
        }
    }
}

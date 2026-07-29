using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;

using AnonymousComplaintsAPI.Controllers;
using AnonymousComplaintsAPI.Services.EnsureServices;
using AnonymousComplaintsAPI.Services.Interfaces;
using AnonymousComplaintsAPI.DTOs.Requests;
using AnonymousComplaintsAPI.DTOs.Responses;
using AnonymousComplaintsAPI.Models.Entities;

namespace TrustLine.Tests.Controllers
{
    public class AnonymousComplaintsControllerTests
    {
        private readonly Mock<IEnsureService> _ensureMock = new();
        private readonly Mock<IAnonymousComplaintService> _complaintMock = new();
        private readonly Mock<ITypeService> _typeMock = new();
        private readonly Mock<IAttachmentService> _attachmentMock = new();

        private AnonymousComplaintsController CreateController(int userId = 1)
        {
            var controller = new AnonymousComplaintsController(
                _ensureMock.Object,
                _complaintMock.Object,
                _typeMock.Object,
                _attachmentMock.Object);

            // Setup HttpContext with authenticated user
            var claims = new[] { new Claim("Id", userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            return controller;
        }

        // =====================================================
        // GET ALL (paginated)
        // =====================================================
        [Fact]
        public async Task GetAnonymousComplaints_ShouldReturnOk()
        {
            var paginated = new PaginatedResponse<AnonymousComplaintResponse>
            {
                Total = 0,
                Data = new List<AnonymousComplaintResponse>(),
                Page = 1,
                PerPage = 10
            };

            _complaintMock.Setup(x => x.GetComplaintsPaginatedAsync(It.IsAny<PaginationRequest>()))
                .ReturnsAsync(paginated);

            var controller = CreateController();
            var result = await controller.GetAnonymousComplaints(new PaginationRequest());

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(ok.Value);
        }

        // =====================================================
        // GET BY ID
        // =====================================================
        [Fact]
        public async Task GetComplaintDetails_ShouldReturnOk()
        {
            _complaintMock.Setup(x => x.GetComplaintDetailsAsync(1))
                .ReturnsAsync(new AnonymousComplaintResponse { Id = 1, Description = "Test" });

            var result = await CreateController().GetComplaintDetails(1);

            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetComplaintDetails_ShouldReturnNotFound()
        {
            _complaintMock.Setup(x => x.GetComplaintDetailsAsync(999))
                .ReturnsAsync((AnonymousComplaintResponse?)null);

            var result = await CreateController().GetComplaintDetails(999);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        // =====================================================
        // CHANGE STATE
        // =====================================================
        [Fact]
        public async Task ChangeState_SubmittedComplaint_ShouldReturnOk()
        {
            _complaintMock.Setup(x => x.GetComplaintAsync(1))
                .ReturnsAsync(new AnonymousComplaintResponse { Id = 1, State = "DÉPOSÉ", Archived = false });

            _complaintMock.Setup(x => x.TransitionComplaintStateAsync(1, "IN PROGRESS"))
                .Returns(Task.CompletedTask);

            var result = await CreateController().ChangeState(1);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task ChangeState_NotFound_ShouldReturnNotFound()
        {
            _complaintMock.Setup(x => x.GetComplaintAsync(999))
                .ReturnsAsync((AnonymousComplaintResponse?)null);

            var result = await CreateController().ChangeState(999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task ChangeState_ArchivedComplaint_ShouldReturnNotFound()
        {
            _complaintMock.Setup(x => x.GetComplaintAsync(1))
                .ReturnsAsync(new AnonymousComplaintResponse { Id = 1, Archived = true });

            var result = await CreateController().ChangeState(1);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        // =====================================================
        // ARCHIVE
        // =====================================================
        [Fact]
        public async Task ArchiveAnonymousComplaint_ShouldReturnNoContent()
        {
            _complaintMock.Setup(x => x.ArchiveComplaintAsync(1))
                .Returns(Task.CompletedTask);

            var result = await CreateController().ArchiveAnonymousComplaint(1);

            Assert.IsType<NoContentResult>(result);
        }

        // =====================================================
        // RESTORE
        // =====================================================
        [Fact]
        public async Task RestoreAnonymousComplaint_ShouldReturnNoContent()
        {
            _complaintMock.Setup(x => x.RestoreComplaintAsync(1))
                .Returns(Task.CompletedTask);

            var result = await CreateController().RestoreAnonymousComplaint(1);

            Assert.IsType<NoContentResult>(result);
        }

        // =====================================================
        // MERGE
        // =====================================================
        [Fact]
        public async Task MergeComplaints_ValidIds_ShouldReturnOk()
        {
            _complaintMock.Setup(x => x.MergeComplaintsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(1);

            var result = await CreateController().MergeComplaints(new List<int> { 1, 2 });

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task MergeComplaints_LessThanTwo_ShouldReturnBadRequest()
        {
            var result = await CreateController().MergeComplaints(new List<int> { 1 });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task MergeComplaints_EmptyList_ShouldReturnBadRequest()
        {
            var result = await CreateController().MergeComplaints(new List<int>());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // =====================================================
        // GET FUSED
        // =====================================================
        [Fact]
        public async Task GetFusedComplaints_ShouldReturnOk()
        {
            _complaintMock.Setup(x => x.GetFusedComplaintsAsync(1))
                .ReturnsAsync(new List<AnonymousComplaintResponse>());

            var result = await CreateController().GetFusedComplaints(1);

            Assert.IsType<OkObjectResult>(result.Result);
        }
    }
}

using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnonymousComplaintsAPI.Services.Implementations;
using AnonymousComplaintsAPI.Repositories.Interfaces;
using AnonymousComplaintsAPI.DTOs.Requests;
using AnonymousComplaintsAPI.Models.Entities;
using System.Linq;
using AnonymousComplaintsAPI.Services.Interfaces;

namespace TrustLine.Tests.Services
{
    public class AnonymousComplaintServiceTests
    {
        private readonly Mock<IAnonymousComplaintRepository> _complaintRepo = new();
        private readonly Mock<ICategoryRepository> _categoryRepo = new();
        private readonly Mock<ITypeRepository> _typeRepo = new();
        private readonly Mock<IAttachmentRepository> _attachmentRepo = new();
        private readonly Mock<ISolutionRepository> _solutionRepo = new();
        private readonly Mock<IFileService> _fileService = new();
        private readonly Mock<ILogger<AnonymousComplaintService>> _logger = new();
        private readonly Mock<AnonymousComplaintsAPI.Services.EnsureServices.IEnsureService> _ensureService = new();
        private readonly Mock<IHrLinkService> _hrLinkService = new();

        private AnonymousComplaintService CreateService()
        {
            return new AnonymousComplaintService(
                _complaintRepo.Object,
                _categoryRepo.Object,
                _typeRepo.Object,
                _attachmentRepo.Object,
                _solutionRepo.Object,
                _fileService.Object,
                _logger.Object,
                _ensureService.Object,
                _hrLinkService.Object
            );
        }

        // =====================================================
        // GET
        // =====================================================

        [Fact]
        public async Task GetComplaintAsync_ShouldReturnComplaint()
        {
            var complaint = new AnonymousComplaint
            {
                AnonymousComplaintId = 1,
                Description = "test"
            };

            _complaintRepo.Setup(x => x.GetWithDetailsAsync(1))
                .ReturnsAsync(complaint);

            var service = CreateService();

            var result = await service.GetComplaintAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetComplaintAsync_ShouldReturnNull_WhenNotFound()
        {
            _complaintRepo.Setup(x => x.GetWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync((AnonymousComplaint?)null);

            var service = CreateService();

            var result = await service.GetComplaintAsync(999);

            Assert.Null(result);
        }

        // =====================================================
        // CREATE
        // =====================================================

        [Fact]
        public async Task CreateComplaint_ShouldThrow_WhenTypeIsMissing()
        {
            var request = new CreateAnonymousComplaintRequest
            {
                Description = "test",
                TypeID = null
            };

            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateComplaintWithAttachmentsAsync(request, null, 1));
        }

        [Fact]
        public async Task CreateComplaint_ShouldThrow_WhenTypeNotExists()
        {
            var request = new CreateAnonymousComplaintRequest
            {
                Description = "test",
                TypeID = 1
            };

            _typeRepo.Setup(x => x.ExistsAsync(1))
                .ReturnsAsync(false);

            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateComplaintWithAttachmentsAsync(request, null, 1));
        }

        [Fact]
        public async Task CreateComplaint_ShouldCreateSuccessfully()
        {
            var request = new CreateAnonymousComplaintRequest
            {
                Description = "test",
                TypeID = 1
            };

            _typeRepo.Setup(x => x.ExistsAsync(1))
                .ReturnsAsync(true);

            _complaintRepo.Setup(x => x.CreateAsync(It.IsAny<AnonymousComplaint>()))
                .ReturnsAsync(new AnonymousComplaint
                {
                    AnonymousComplaintId = 1,
                    Description = "test"
                });

            _complaintRepo.Setup(x => x.GetWithDetailsAsync(1))
                .ReturnsAsync(new AnonymousComplaint
                {
                    AnonymousComplaintId = 1,
                    Description = "test"
                });

            _fileService.Setup(x => x.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync("fake-path");

            var service = CreateService();

            var result = await service.CreateComplaintWithAttachmentsAsync(request, null, 1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        // =====================================================
        // STATE TRANSITION
        // =====================================================

        [Fact]
        public async Task TransitionComplaint_ShouldUpdateState()
        {
            var complaint = new AnonymousComplaint
            {
                AnonymousComplaintId = 1,
                Archived = false
            };

            _complaintRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(complaint);

            _complaintRepo.Setup(x => x.UpdateStateAsync(1, "RESOLVED"))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            await service.TransitionComplaintStateAsync(1, "RESOLVED");

            _complaintRepo.Verify(x => x.UpdateStateAsync(1, "RESOLVED"), Times.Once);
        }

        [Fact]
        public async Task TransitionComplaint_ShouldThrow_WhenArchived()
        {
            var complaint = new AnonymousComplaint
            {
                AnonymousComplaintId = 1,
                Archived = true
            };

            _complaintRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(complaint);

            var service = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.TransitionComplaintStateAsync(1, "RESOLVED"));
        }

        // =====================================================
        // ADD RESPONSE
        // =====================================================

        [Fact]
        public async Task AddResponse_ShouldCreateSolution_AndResolveComplaint()
        {
            var request = new SendResponseRequest
            {
                AnonymousComplaintID = 1,
                Content = "solution"
            };

            _complaintRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new AnonymousComplaint { AnonymousComplaintId = 1 });

            _solutionRepo.Setup(x => x.CreateAsync(It.IsAny<Solution>()))
                .ReturnsAsync(new Solution
                {
                    SolutionId = 10,
                    Content = "solution"
                });

            _complaintRepo.Setup(x => x.UpdateStateAsync(1, "RESOLVED"))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            var result = await service.AddComplaintResponseAsync(request);

            Assert.NotNull(result);
            Assert.Equal("solution", result.Content);

            _complaintRepo.Verify(x => x.UpdateStateAsync(1, "RESOLVED"), Times.Once);
        }

        // =====================================================
        // MERGE
        // =====================================================

        [Fact]
        public async Task MergeComplaints_ShouldThrow_WhenLessThan2()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.MergeComplaintsAsync(new List<int> { 1 }));
        }
    }
}
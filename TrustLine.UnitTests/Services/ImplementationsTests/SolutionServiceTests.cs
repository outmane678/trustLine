using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using AnonymousComplaintsAPI.Services.Implementations;
using AnonymousComplaintsAPI.Repositories.Interfaces;
using AnonymousComplaintsAPI.DTOs.Requests;
using AnonymousComplaintsAPI.DTOs.Responses;
using AnonymousComplaintsAPI.Models.Entities;

namespace TrustLine.Tests.Services
{
    public class SolutionServiceTests
    {
        private readonly Mock<ISolutionRepository> _solutionRepo = new();
        private readonly Mock<IAnonymousComplaintRepository> _complaintRepo = new();
        private readonly Mock<ILogger<SolutionService>> _logger = new();

        private SolutionService CreateService()
        {
            return new SolutionService(
                _solutionRepo.Object,
                _complaintRepo.Object,
                _logger.Object
            );
        }

        // =========================
        // GET
        // =========================

        [Fact]
        public async Task GetSolution_ShouldReturnSolution()
        {
            var solution = new Solution
            {
                SolutionId = 1,
                Content = "test"
            };

            _solutionRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(solution);

            var service = CreateService();

            var result = await service.GetSolutionAsync(1);

            Assert.NotNull(result);
            Assert.Equal("test", result!.Content);
        }

        [Fact]
        public async Task GetSolution_ShouldReturnNull_WhenNotFound()
        {
            _solutionRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Solution?)null);

            var service = CreateService();

            var result = await service.GetSolutionAsync(99);

            Assert.Null(result);
        }

        // =========================
        // CREATE
        // =========================

        [Fact]
        public async Task CreateSolution_ShouldThrow_WhenComplaintIdMissing()
        {
            var dto = new SendResponseRequest
            {
                Content = "test",
                AnonymousComplaintID = null
            };

            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateSolutionAsync(dto));
        }

        [Fact]
        public async Task CreateSolution_ShouldCreate_AndResolveComplaint()
        {
            var dto = new SendResponseRequest
            {
                Content = "test",
                AnonymousComplaintID = 1
            };

            _solutionRepo.Setup(x => x.CreateAsync(It.IsAny<Solution>()))
                .ReturnsAsync(new Solution
                {
                    SolutionId = 1,
                    Content = "test"
                });

            _complaintRepo.Setup(x => x.UpdateStateAsync(1, "RESOLVED"))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            var result = await service.CreateSolutionAsync(dto);

            Assert.NotNull(result);
            Assert.Equal("test", result.Content);

            _complaintRepo.Verify(x => x.UpdateStateAsync(1, "RESOLVED"), Times.Once);
        }

        // =========================
        // UPDATE
        // =========================

        [Fact]
        public async Task UpdateSolution_ShouldUpdateContent()
        {
            var solution = new Solution
            {
                SolutionId = 1,
                Content = "old"
            };

            _solutionRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(solution);

            _solutionRepo.Setup(x => x.UpdateAsync(solution))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            var dto = new SendResponseRequest
            {
                Content = "new"
            };

            var result = await service.UpdateSolutionAsync(1, dto);

            Assert.Equal("new", result.Content);
        }

        [Fact]
        public async Task UpdateSolution_ShouldThrow_WhenNotFound()
        {
            _solutionRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Solution?)null);

            var service = CreateService();

            var dto = new SendResponseRequest { Content = "test" };

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.UpdateSolutionAsync(1, dto));
        }

        // =========================
        // DELETE / ARCHIVE
        // =========================

        [Fact]
        public async Task ArchiveSolution_ShouldCallRepository()
        {
            _solutionRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Solution { SolutionId = 1 });
            var service = CreateService();

            await service.ArchiveSolutionAsync(1);

            _solutionRepo.Verify(x => x.ArchiveAsync(1), Times.Once);
        }
    }
}
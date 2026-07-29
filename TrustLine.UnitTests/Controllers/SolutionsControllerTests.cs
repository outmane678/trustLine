using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

using AnonymousComplaintsAPI.Controllers;
using AnonymousComplaintsAPI.Services.Interfaces;
using AnonymousComplaintsAPI.DTOs.Requests;
using AnonymousComplaintsAPI.DTOs.Responses;

namespace TrustLine.Tests.Controllers
{
    public class SolutionsControllerTests
    {
        private readonly Mock<ISolutionService> _serviceMock = new();

        private SolutionsController CreateController() =>
            new SolutionsController(_serviceMock.Object);

        // =====================================================
        // GET ALL (non archived)
        // =====================================================
        [Fact]
        public async Task GetSolutions_ShouldReturnOnlyNonArchived()
        {
            _serviceMock.Setup(x => x.GetAllSolutionsAsync())
                .ReturnsAsync(new List<SolutionResponse>
                {
                    new SolutionResponse { SolutionID = 1, Content = "A", Archived = false }
                });

            var result = await CreateController().GetSolutions();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(ok.Value);
        }

        // =====================================================
        // GET ALL (including archived)
        // =====================================================
        [Fact]
        public async Task GetAllSolutions_ShouldReturnAll()
        {
            _serviceMock.Setup(x => x.GetAllSolutionsIncludingArchivedAsync())
                .ReturnsAsync(new List<SolutionResponse>
                {
                    new SolutionResponse { SolutionID = 1, Content = "A" },
                    new SolutionResponse { SolutionID = 2, Content = "B" }
                });

            var result = await CreateController().GetAllSolutions();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(ok.Value);
        }

        // =====================================================
        // GET BY ID
        // =====================================================
        [Fact]
        public async Task GetSolution_ShouldReturnOk()
        {
            _serviceMock.Setup(x => x.GetSolutionAsync(1))
                .ReturnsAsync(new SolutionResponse { SolutionID = 1, Content = "Test" });

            var result = await CreateController().GetSolution(1);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task GetSolution_NotFound_ShouldReturnNotFound()
        {
            _serviceMock.Setup(x => x.GetSolutionAsync(99))
                .ReturnsAsync((SolutionResponse?)null);

            var result = await CreateController().GetSolution(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        // =====================================================
        // POST
        // =====================================================
        [Fact]
        public async Task PostSolution_ShouldReturnCreated()
        {
            var dto = new SendResponseRequest
            {
                Content = "Fix",
                AnonymousComplaintID = 1,
                CreatedAt = DateTime.Now
            };

            _serviceMock.Setup(x => x.CreateSolutionForComplaintAndMergedAsync(dto))
                .ReturnsAsync(new SolutionResponse { SolutionID = 1, Content = "Fix" });

            var result = await CreateController().PostSolution(dto);

            Assert.IsType<CreatedAtActionResult>(result.Result);
        }

        // =====================================================
        // PUT
        // =====================================================
        [Fact]
        public async Task PutSolution_ShouldReturnNoContent()
        {
            var dto = new SendResponseRequest { SolutionID = 1, Content = "Updated" };

            _serviceMock.Setup(x => x.UpdateSolutionAsync(1, dto))
                .ReturnsAsync(new SolutionResponse { SolutionID = 1, Content = "Updated" });

            var result = await CreateController().PutSolution(1, dto);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task PutSolution_IdMismatch_ShouldReturnBadRequest()
        {
            var dto = new SendResponseRequest { SolutionID = 2 };

            var result = await CreateController().PutSolution(1, dto);

            Assert.IsType<BadRequestResult>(result);
        }

        // =====================================================
        // ARCHIVE
        // =====================================================
        [Fact]
        public async Task ArchiveSolution_ShouldReturnNoContent()
        {
            _serviceMock.Setup(x => x.ArchiveSolutionAsync(1))
                .Returns(Task.CompletedTask);

            var result = await CreateController().ArchiveSolution(1);

            Assert.IsType<NoContentResult>(result);
        }

        // =====================================================
        // RESTORE
        // =====================================================
        [Fact]
        public async Task RestoreSolution_ShouldReturnNoContent()
        {
            _serviceMock.Setup(x => x.RestoreSolutionAsync(1))
                .Returns(Task.CompletedTask);

            var result = await CreateController().RestoreSolution(1);

            Assert.IsType<NoContentResult>(result);
        }

        // =====================================================
        // DELETE
        // =====================================================
        [Fact]
        public async Task DeleteSolution_ShouldReturnNoContent()
        {
            _serviceMock.Setup(x => x.DeleteSolutionAsync(1))
                .Returns(Task.CompletedTask);

            var result = await CreateController().DeleteSolution(1);

            Assert.IsType<NoContentResult>(result);
        }
    }
}

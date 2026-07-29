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
    public class FrequenciesControllerTests
    {
        private readonly Mock<IFrequencyService> _serviceMock = new();

        private FrequenciesController CreateController() =>
            new FrequenciesController(_serviceMock.Object);

        // =====================================================
        // GET ALL
        // =====================================================
        [Fact]
        public async Task GetFrequencies_ShouldReturnOk()
        {
            _serviceMock.Setup(x => x.GetAllFrequenciesAsync(It.IsAny<bool>()))
                .ReturnsAsync(new List<FrequencyResponse>
                {
                    new FrequencyResponse { FrequencyID = 1, Label = "Daily" },
                    new FrequencyResponse { FrequencyID = 2, Label = "Weekly" }
                });

            var result = await CreateController().GetFrequencies();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(ok.Value);
        }

        // =====================================================
        // GET BY ID
        // =====================================================
        [Fact]
        public async Task GetFrequency_ShouldReturnOk()
        {
            _serviceMock.Setup(x => x.GetFrequencyAsync(1))
                .ReturnsAsync(new FrequencyResponse { FrequencyID = 1, Label = "Monthly" });

            var result = await CreateController().GetFrequency(1);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task GetFrequency_NotFound_ShouldReturnNotFound()
        {
            _serviceMock.Setup(x => x.GetFrequencyAsync(99))
                .ReturnsAsync((FrequencyResponse?)null);

            var result = await CreateController().GetFrequency(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        // =====================================================
        // CREATE
        // =====================================================
        [Fact]
        public async Task PostFrequency_ShouldReturnCreated()
        {
            var dto = new FrequencyResponse { Label = "Yearly" };
            var created = new FrequencyResponse { FrequencyID = 1, Label = "Yearly" };

            _serviceMock.Setup(x => x.CreateFrequencyAsync(dto))
                .ReturnsAsync(created);

            var result = await CreateController().PostFrequency(dto);

            Assert.IsType<CreatedAtActionResult>(result.Result);
        }

        // =====================================================
        // UPDATE
        // =====================================================
        [Fact]
        public async Task PutFrequency_ShouldReturnNoContent()
        {
            var dto = new FrequencyResponse { FrequencyID = 1, Label = "Updated" };

            _serviceMock.Setup(x => x.UpdateFrequencyAsync(1, dto))
                .ReturnsAsync(new FrequencyResponse { FrequencyID = 1, Label = "Updated" });

            var result = await CreateController().PutFrequency(1, dto);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task PutFrequency_IdMismatch_ShouldReturnBadRequest()
        {
            var dto = new FrequencyResponse { FrequencyID = 2 };

            var result = await CreateController().PutFrequency(1, dto);

            Assert.IsType<BadRequestResult>(result);
        }

        // =====================================================
        // ARCHIVE
        // =====================================================
        [Fact]
        public async Task ArchiveFrequency_ShouldReturnNoContent()
        {
            _serviceMock.Setup(x => x.ArchiveFrequencyAsync(1))
                .Returns(Task.CompletedTask);

            var result = await CreateController().ArchiveFrequency(1);

            Assert.IsType<NoContentResult>(result);
        }

        // =====================================================
        // RESTORE
        // =====================================================
        [Fact]
        public async Task RestoreFrequency_ShouldReturnNoContent()
        {
            _serviceMock.Setup(x => x.RestoreFrequencyAsync(1))
                .Returns(Task.CompletedTask);

            var result = await CreateController().RestoreFrequency(1);

            Assert.IsType<NoContentResult>(result);
        }

        // =====================================================
        // DELETE
        // =====================================================
        [Fact]
        public async Task DeleteFrequency_ShouldReturnNoContent()
        {
            _serviceMock.Setup(x => x.DeleteFrequencyAsync(1))
                .Returns(Task.CompletedTask);

            var result = await CreateController().DeleteFrequency(1);

            Assert.IsType<NoContentResult>(result);
        }
    }
}

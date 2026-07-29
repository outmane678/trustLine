using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnonymousComplaintsAPI.Services.Implementations;
using AnonymousComplaintsAPI.Repositories.Interfaces;
using AnonymousComplaintsAPI.Models.Entities;
using AnonymousComplaintsAPI.DTOs.Responses;

namespace TrustLine.Tests.Services
{
    public class FrequencyServiceTests
    {
        private readonly Mock<IFrequencyRepository> _repo = new();
        private readonly Mock<ILogger<FrequencyService>> _logger = new();

        private FrequencyService CreateService()
        {
            return new FrequencyService(_repo.Object, _logger.Object);
        }

        // =========================
        // GET ALL
        // =========================

        [Fact]
        public async Task GetAll_ShouldReturnList()
        {
            _repo.Setup(x => x.GetNonArchivedAsync())
                .ReturnsAsync(new List<Frequency>
                {
                    new Frequency { FrequencyId = 1, Label = "Daily" }
                });

            var service = CreateService();

            var result = await service.GetAllFrequenciesAsync();

            Assert.NotNull(result);
            Assert.Single(result);
        }

        // =========================
        // GET BY ID
        // =========================

        [Fact]
        public async Task GetById_ShouldReturnFrequency()
        {
            _repo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new Frequency
                {
                    FrequencyId = 1,
                    Label = "Weekly"
                });

            var service = CreateService();

            var result = await service.GetFrequencyAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Weekly", result!.Label);
        }

        [Fact]
        public async Task GetById_ShouldReturnNull_WhenNotFound()
        {
            _repo.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Frequency?)null);

            var service = CreateService();

            var result = await service.GetFrequencyAsync(999);

            Assert.Null(result);
        }

        // =========================
        // CREATE
        // =========================

        [Fact]
        public async Task Create_ShouldReturnCreatedFrequency()
        {
            var dto = new FrequencyResponse
            {
                Label = "Monthly",
                CreatedBy = 1
            };

            _repo.Setup(x => x.CreateAsync(It.IsAny<Frequency>()))
                .ReturnsAsync((Frequency f) =>
                {
                    f.FrequencyId = 1;
                    return f;
                });

            var service = CreateService();

            var result = await service.CreateFrequencyAsync(dto);

            Assert.NotNull(result);
            Assert.Equal("Monthly", result.Label);
        }

        // =========================
        // UPDATE
        // =========================

        [Fact]
        public async Task Update_ShouldUpdateFrequency()
        {
            var existing = new Frequency
            {
                FrequencyId = 1,
                Label = "Old"
            };

            _repo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(existing);

            var dto = new FrequencyResponse
            {
                Label = "New"
            };

            var service = CreateService();

            var result = await service.UpdateFrequencyAsync(1, dto);

            Assert.Equal("New", result.Label);

            _repo.Verify(x => x.UpdateAsync(existing), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldThrow_WhenNotFound()
        {
            _repo.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Frequency?)null);

            var service = CreateService();

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.UpdateFrequencyAsync(1, new FrequencyResponse()));
        }

        // =========================
        // ARCHIVE / RESTORE / DELETE
        // =========================

        [Fact]
        public async Task Archive_ShouldCallRepository()
        {
            _repo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Frequency { FrequencyId = 1, Label = "Test" });
            var service = CreateService();

            await service.ArchiveFrequencyAsync(1);

            _repo.Verify(x => x.ArchiveAsync(1), Times.Once);
        }

        [Fact]
        public async Task Restore_ShouldCallRepository()
        {
            _repo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Frequency { FrequencyId = 1, Label = "Test" });
            var service = CreateService();

            await service.RestoreFrequencyAsync(1);

            _repo.Verify(x => x.RestoreAsync(1), Times.Once);
        }

        [Fact]
        public async Task Delete_ShouldCallRepository()
        {
            _repo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Frequency { FrequencyId = 1, Label = "Test" });
            var service = CreateService();

            await service.DeleteFrequencyAsync(1);

            _repo.Verify(x => x.DeleteAsync(1), Times.Once);
        }
    }
}
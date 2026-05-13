using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using AnonymousComplaintsAPI.Controllers;
using AnonymousComplaintsAPI.Data;
using AnonymousComplaintsAPI.Models.Entities;
using AnonymousComplaintsAPI.DTOs.Responses;

namespace TrustLine.Tests.Controllers
{
    public class FrequenciesControllerTests
    {
        private AnonymousComplaintsV002Context CreateDb()
        {
            var options = new DbContextOptionsBuilder<AnonymousComplaintsV002Context>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AnonymousComplaintsV002Context(options);
        }

        private FrequenciesController CreateController(AnonymousComplaintsV002Context context)
        {
            return new FrequenciesController(context);
        }

        // =========================
        // GET ALL
        // =========================
        [Fact]
        public async Task GetFrequencies_ShouldReturnList()
        {
            var context = CreateDb();

            context.Frequencies.Add(new Frequency { FrequencyId = 1, Label = "Daily" });
            context.Frequencies.Add(new Frequency { FrequencyId = 2, Label = "Weekly" });
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var result = await controller.GetFrequencies();

            var list = Assert.IsAssignableFrom<IEnumerable<FrequencyResponse>>(result.Value);
            Assert.Equal(2, list.Count());
        }

        // =========================
        // GET BY ID
        // =========================
        [Fact]
        public async Task GetFrequency_ShouldReturnFrequency()
        {
            var context = CreateDb();

            context.Frequencies.Add(new Frequency
            {
                FrequencyId = 1,
                Label = "Monthly"
            });
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var result = await controller.GetFrequency(1);

            Assert.NotNull(result.Value);
            Assert.Equal("Monthly", result.Value.Label);
        }

        [Fact]
        public async Task GetFrequency_ShouldReturnNotFound()
        {
            var context = CreateDb();
            var controller = CreateController(context);

            var result = await controller.GetFrequency(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        // =========================
        // POST
        // =========================
        [Fact]
        public async Task PostFrequency_ShouldCreateFrequency()
        {
            var context = CreateDb();
            var controller = CreateController(context);

            var dto = new FrequencyResponse
            {
                Label = "Yearly",
                CreatedBy = 1,
                CreatedAt = DateTime.Now,
                Archived = false
            };

            var result = await controller.PostFrequency(dto);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var value = Assert.IsType<FrequencyResponse>(created.Value);

            Assert.Equal("Yearly", value.Label);
            Assert.Single(context.Frequencies);
        }

        // =========================
        // PUT
        // =========================
        [Fact]
        public async Task PutFrequency_ShouldUpdate()
        {
            var context = CreateDb();

            context.Frequencies.Add(new Frequency
            {
                FrequencyId = 1,
                Label = "Old"
            });
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var dto = new FrequencyResponse
            {
                FrequencyID = 1,
                Label = "Updated"
            };

            var result = await controller.PutFrequency(1, dto);

            Assert.IsType<NoContentResult>(result);

            var updated = await context.Frequencies.FindAsync(1);
            Assert.Equal("Updated", updated.Label);
        }

        [Fact]
        public async Task PutFrequency_ShouldReturnBadRequest()
        {
            var context = CreateDb();
            var controller = CreateController(context);

            var dto = new FrequencyResponse { FrequencyID = 2 };

            var result = await controller.PutFrequency(1, dto);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task PutFrequency_ShouldReturnNotFound()
        {
            var context = CreateDb();
            var controller = CreateController(context);

            var dto = new FrequencyResponse { FrequencyID = 1 };

            var result = await controller.PutFrequency(1, dto);

            Assert.IsType<NotFoundResult>(result);
        }

        // =========================
        // ARCHIVE
        // =========================
        [Fact]
        public async Task ArchiveFrequency_ShouldArchive()
        {
            var context = CreateDb();

            context.Frequencies.Add(new Frequency { FrequencyId = 1, Label = "A", Archived = false });
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var result = await controller.ArchiveFrequency(1);

            Assert.IsType<NoContentResult>(result);

            var freq = await context.Frequencies.FindAsync(1);
            Assert.True(freq.Archived);
        }

        // =========================
        // RESTORE
        // =========================
        [Fact]
        public async Task RestoreFrequency_ShouldRestore()
        {
            var context = CreateDb();

            context.Frequencies.Add(new Frequency { FrequencyId = 1, Label = "A", Archived = true });
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var result = await controller.RestoreFrequency(1);

            Assert.IsType<NoContentResult>(result);

            var freq = await context.Frequencies.FindAsync(1);
            Assert.False(freq.Archived);
        }

        // =========================
        // DELETE
        // =========================
        [Fact]
        public async Task DeleteFrequency_ShouldDelete()
        {
            var context = CreateDb();

            context.Frequencies.Add(new Frequency { FrequencyId = 1, Label = "ToDelete" });
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var result = await controller.DeleteFrequency(1);

            Assert.IsType<NoContentResult>(result);
            Assert.Empty(context.Frequencies);
        }

        [Fact]
        public async Task DeleteFrequency_ShouldReturnNotFound()
        {
            var context = CreateDb();
            var controller = CreateController(context);

            var result = await controller.DeleteFrequency(1);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
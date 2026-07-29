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
    public class SolutionsControllerTests
    {
        private AnonymousComplaintsV002Context CreateDb()
        {
            var options = new DbContextOptionsBuilder<AnonymousComplaintsV002Context>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AnonymousComplaintsV002Context(options);
        }

        private SolutionsController CreateController(AnonymousComplaintsV002Context context)
        {
            return new SolutionsController(context);
        }

        // =========================
        // GET ALL (non archived)
        // =========================
        [Fact]
        public async Task GetSolutions_ShouldReturnOnlyNonArchived()
        {
            var context = CreateDb();

            context.Solutions.Add(new Solution { SolutionId = 1, Content = "A", Archived = false });
            context.Solutions.Add(new Solution { SolutionId = 2, Content = "B", Archived = true });
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var result = await controller.GetSolutions();

            var list = result.Value.ToList();
            Assert.Single(list);
        }

        // =========================
        // GET ALL (including archived)
        // =========================
        [Fact]
        public async Task GetAllSolutions_ShouldReturnAll()
        {
            var context = CreateDb();

            context.Solutions.Add(new Solution { SolutionId = 1, Content = "A" });
            context.Solutions.Add(new Solution { SolutionId = 2, Content = "B" });
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var result = await controller.GetAllSolutions();

            Assert.Equal(2, result.Value.Count());
        }

        // =========================
        // GET BY ID
        // =========================
        [Fact]
        public async Task GetSolution_ShouldReturnSolution()
        {
            var context = CreateDb();

            context.Solutions.Add(new Solution { SolutionId = 1, Content = "Test" });
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var result = await controller.GetSolution(1);

            Assert.NotNull(result.Value);
            Assert.Equal("Test", result.Value.Content);
        }

        [Fact]
        public async Task GetSolution_ShouldReturnNotFound()
        {
            var context = CreateDb();
            var controller = CreateController(context);

            var result = await controller.GetSolution(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        // =========================
        // POST (🔥 IMPORTANT)
        // =========================
        [Fact]
        public async Task PostSolution_ShouldCreateSolution_AndUpdateComplaintState()
        {
            var context = CreateDb();

            context.AnonymousComplaints.Add(new AnonymousComplaint
            {
                AnonymousComplaintId = 1,
                State = "SUBMITTED",
                Description = "Test"
            });

            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var dto = new SolutionResponse
            {
                Content = "Fix",
                AnonymousComplaintID = 1,
                CreatedBy = 1,
                CreatedAt = DateTime.Now
            };

            var result = await controller.PostSolution(dto);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);

            // Vérifier solution créée
            Assert.Single(context.Solutions);

            // Vérifier état changé
            var complaint = await context.AnonymousComplaints.FindAsync(1);
            Assert.Equal("RESOLVED", complaint.State);
        }

        // =========================
        // POST WITH MERGED
        // =========================
        [Fact]
        public async Task PostSolution_ShouldCreateSolutions_ForMergedComplaints()
        {
            var context = CreateDb();

            // plainte principale
            context.AnonymousComplaints.Add(new AnonymousComplaint
            {
                AnonymousComplaintId = 1,
                State = "SUBMITTED",
                Description = "Main"
            });

            // plainte fusionnée
            context.AnonymousComplaints.Add(new AnonymousComplaint
            {
                AnonymousComplaintId = 2,
                FusionWithId = 1,
                State = "SUBMITTED",
                Description = "Merged"
            });

            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var dto = new SolutionResponse
            {
                Content = "Fix",
                AnonymousComplaintID = 1,
                CreatedBy = 1,
                CreatedAt = DateTime.Now
            };

            await controller.PostSolution(dto);

            // 2 solutions doivent être créées
            Assert.Equal(2, context.Solutions.Count());

            // états mis à jour
            var complaints = context.AnonymousComplaints.ToList();
            Assert.All(complaints, c => Assert.Equal("RESOLVED", c.State));
        }

        // =========================
        // PUT
        // =========================
        [Fact]
        public async Task PutSolution_ShouldUpdate()
        {
            var context = CreateDb();

            context.Solutions.Add(new Solution
            {
                SolutionId = 1,
                Content = "Old"
            });

            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var dto = new SolutionResponse
            {
                SolutionID = 1,
                Content = "Updated"
            };

            var result = await controller.PutSolution(1, dto);

            Assert.IsType<NoContentResult>(result);

            var updated = await context.Solutions.FindAsync(1);
            Assert.Equal("Updated", updated.Content);
        }

        // =========================
        // ARCHIVE
        // =========================
        [Fact]
        public async Task ArchiveSolution_ShouldArchive()
        {
            var context = CreateDb();

            context.Solutions.Add(new Solution { SolutionId = 1, Content = "A", Archived = false });
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var result = await controller.ArchiveSolution(1);

            Assert.IsType<NoContentResult>(result);

            var solution = await context.Solutions.FindAsync(1);
            Assert.True(solution.Archived);
        }

        // =========================
        // RESTORE
        // =========================
        [Fact]
        public async Task RestoreSolution_ShouldRestore()
        {
            var context = CreateDb();

            context.Solutions.Add(new Solution { SolutionId = 1, Content = "A", Archived = true });
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var result = await controller.RestoreSolution(1);

            Assert.IsType<NoContentResult>(result);

            var solution = await context.Solutions.FindAsync(1);
            Assert.False(solution.Archived);
        }

        // =========================
        // DELETE
        // =========================
        [Fact]
        public async Task DeleteSolution_ShouldDelete()
        {
            var context = CreateDb();

            context.Solutions.Add(new Solution { SolutionId = 1, Content = "A" });
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var result = await controller.DeleteSolution(1);

            Assert.IsType<NoContentResult>(result);
            Assert.Empty(context.Solutions);
        }
    }
}
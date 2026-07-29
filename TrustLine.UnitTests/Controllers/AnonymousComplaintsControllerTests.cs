using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AnonymousComplaintsAPI.Controllers;
using AnonymousComplaintsAPI.Data;
using AnonymousComplaintsAPI.Models.Entities;
using AnonymousComplaintsAPI.Repositories.Interfaces;
using AnonymousComplaintsAPI.Services.EnsureServices;
using AnonymousComplaintsAPI.DTOs.Requests;
using AnonymousComplaintsAPI.DTOs.Responses;

namespace TrustLine.Tests.Controllers
{
    public class AnonymousComplaintsControllerTests
    {
        // =========================
        // DB IN MEMORY
        // =========================
        private AnonymousComplaintsV002Context CreateDb()
        {
            var options = new DbContextOptionsBuilder<AnonymousComplaintsV002Context>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AnonymousComplaintsV002Context(options);
        }

        // =========================
        // MOCK ENSURE SERVICE
        // =========================
        private Mock<IEnsureService> CreateEnsureService()
        {
            var mock = new Mock<IEnsureService>();

            mock.Setup(x => x.EnsureUserExistsAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => new User { UserId = id });

            return mock;
        }

        // =========================
        // MOCK REPOSITORY
        // =========================
        private Mock<IAnonymousComplaintRepository> CreateRepo()
        {
            return new Mock<IAnonymousComplaintRepository>();
        }

        // =========================
        // CONTROLLER FACTORY
        // =========================
        private AnonymousComplaintsController CreateController(
            AnonymousComplaintsV002Context context,
            IAnonymousComplaintRepository repo,
            IEnsureService ensure)
        {
            return new AnonymousComplaintsController(context, repo, ensure);
        }

        // =====================================================
        // GET ALL
        // =====================================================
        [Fact]
        public async Task GetAll_ShouldReturnOk()
        {
            var context = CreateDb();

            context.AnonymousComplaints.Add(new AnonymousComplaint
            {
                AnonymousComplaintId = 1,
                Description = "Test"
            });

            await context.SaveChangesAsync();

            var repo = CreateRepo();
            repo.Setup(x => x.GetAllWithDetailsAsync())
                .ReturnsAsync(context.AnonymousComplaints.ToList());

            var ensure = CreateEnsureService();

            var controller = CreateController(context, repo.Object, ensure.Object);

            var result = await controller.GetAnonymousComplaints();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsAssignableFrom<IEnumerable<AnonymousComplaintResponse>>(ok.Value).ToList();

            Assert.Single(data);
        }

        // =====================================================
        // GET BY USER
        // =====================================================
        [Fact]
        public async Task GetByUser_ShouldReturnOk()
        {
            var context = CreateDb();

            context.AnonymousComplaints.Add(new AnonymousComplaint
            {
                AnonymousComplaintId = 1,
                CreatedBy = 10,
                Description = "Test"
            });

            await context.SaveChangesAsync();

            var repo = CreateRepo();

            repo.Setup(x => x.GetByUserIdAsync(10))
                .ReturnsAsync(context.AnonymousComplaints.ToList());

            repo.Setup(x => x.GetWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) =>
                    context.AnonymousComplaints.FirstOrDefault(x => x.AnonymousComplaintId == id));

            var ensure = CreateEnsureService();

            var controller = CreateController(context, repo.Object, ensure.Object);

            var result = await controller.GetAnonymousComplaintsByUser(10);

            var ok = Assert.IsType<OkObjectResult>(result.Result);

            Assert.NotNull(ok.Value);
        }

        // =====================================================
        // CHANGE STATE
        // =====================================================
        [Fact]
        public async Task ChangeState_ShouldReturnOk()
        {
            var context = CreateDb();

            context.AnonymousComplaints.Add(new AnonymousComplaint
            {
                AnonymousComplaintId = 1,
                State = "SUBMITTED",
                Archived = false,
                Description = "Test"
            });

            await context.SaveChangesAsync();

            var repo = CreateRepo();

            repo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(context.AnonymousComplaints.First());

            repo.Setup(x => x.UpdateStateAsync(1, "IN PROGRESS"))
                .Returns(Task.CompletedTask);

            var ensure = CreateEnsureService();

            var controller = CreateController(context, repo.Object, ensure.Object);

            var result = await controller.ChangeState(1);

            Assert.IsType<OkResult>(result);
        }

        // =====================================================
        // ARCHIVE
        // =====================================================
        [Fact]
        public async Task Archive_ShouldSetArchivedTrue()
        {
            var context = CreateDb();

            context.AnonymousComplaints.Add(new AnonymousComplaint
            {
                AnonymousComplaintId = 1,
                Archived = false,
                Description = "Test"
            });

            await context.SaveChangesAsync();

            var repo = CreateRepo();
            var ensure = CreateEnsureService();

            var controller = CreateController(context, repo.Object, ensure.Object);

            var result = await controller.ArchiveAnonymousComplaint(1);

            Assert.IsType<NoContentResult>(result);

            var entity = await context.AnonymousComplaints.FindAsync(1);
            Assert.True(entity!.Archived);
        }

        // =====================================================
        // RESTORE
        // =====================================================
        [Fact]
        public async Task Restore_ShouldSetArchivedFalse()
        {
            var context = CreateDb();

            context.AnonymousComplaints.Add(new AnonymousComplaint
            {
                AnonymousComplaintId = 1,
                Archived = true,
                Description = "Test"
            });

            await context.SaveChangesAsync();

            var repo = CreateRepo();
            var ensure = CreateEnsureService();

            var controller = CreateController(context, repo.Object, ensure.Object);

            var result = await controller.RestoreAnonymousComplaint(1);

            Assert.IsType<NoContentResult>(result);

            var entity = await context.AnonymousComplaints.FindAsync(1);
            Assert.False(entity!.Archived);
        }

        // =====================================================
        // MERGE
        // =====================================================
        [Fact]
        public async Task Merge_ShouldReturnOk()
        {
            var context = CreateDb();

            context.AnonymousComplaints.AddRange(
                new AnonymousComplaint
                {
                    AnonymousComplaintId = 1,
                    CreatedAt = DateTime.Now,
                    Description = "Complaint A"
                },
                new AnonymousComplaint
                {
                    AnonymousComplaintId = 2,
                    CreatedAt = DateTime.Now.AddMinutes(-10),
                    Description = "Complaint B"
                }
            );

            await context.SaveChangesAsync();

            var repo = CreateRepo();
            var ensure = CreateEnsureService();

            var controller = CreateController(context, repo.Object, ensure.Object);

            var result = await controller.MergeComplaints(new List<int> { 1, 2 });

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }
    }
}
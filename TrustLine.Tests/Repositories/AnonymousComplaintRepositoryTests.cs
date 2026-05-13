using Xunit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using AnonymousComplaintsAPI.Data;
using AnonymousComplaintsAPI.Models.Entities;
using AnonymousComplaintsAPI.Repositories.Implementations;

namespace TrustLine.Tests.Repositories
{
    public class AnonymousComplaintRepositoryTests
    {
        private AnonymousComplaintsV002Context CreateDb()
        {
            var options = new DbContextOptionsBuilder<AnonymousComplaintsV002Context>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AnonymousComplaintsV002Context(options);
        }

        private AnonymousComplaintRepository CreateRepo(AnonymousComplaintsV002Context context)
        {
            return new AnonymousComplaintRepository(context);
        }

        // ============================
        // ✅ CREATE
        // ============================
        [Fact]
        public async Task CreateAsync_AddsComplaint()
        {
            var context = CreateDb();
            var repo = CreateRepo(context);

            var complaint = new AnonymousComplaint
            {
                Description = "Test"
            };

            await repo.CreateAsync(complaint);

            Assert.Equal(1, context.AnonymousComplaints.Count());
        }

        // ============================
        // ✅ GET ALL
        // ============================
        [Fact]
        public async Task GetAllAsync_ReturnsAll()
        {
            var context = CreateDb();

            context.AnonymousComplaints.Add(new AnonymousComplaint { Description = "A" });
            context.AnonymousComplaints.Add(new AnonymousComplaint { Description = "B" });

            await context.SaveChangesAsync();

            var repo = CreateRepo(context);

            var result = await repo.GetAllAsync();

            Assert.Equal(2, result.Count());
        }

        // ============================
        // ✅ GET BY ID
        // ============================
        [Fact]
        public async Task GetByIdAsync_ReturnsCorrect()
        {
            var context = CreateDb();

            context.AnonymousComplaints.Add(new AnonymousComplaint
            {
                AnonymousComplaintId = 1,
                Description = "Test"
            });

            await context.SaveChangesAsync();

            var repo = CreateRepo(context);

            var result = await repo.GetByIdAsync(1);

            Assert.NotNull(result);
        }

        // ============================
        // ✅ EXISTS
        // ============================
        [Fact]
        public async Task ExistsAsync_ReturnsTrue()
        {
            var context = CreateDb();

            context.AnonymousComplaints.Add(new AnonymousComplaint
            {
                AnonymousComplaintId = 1,
                Description = "Test"
            });

            await context.SaveChangesAsync();

            var repo = CreateRepo(context);

            var result = await repo.ExistsAsync(1);

            Assert.True(result);
        }

        // ============================
        // ✅ GET BY USER
        // ============================
        [Fact]
        public async Task GetByUserIdAsync_ReturnsFiltered()
        {
            var context = CreateDb();

            context.AnonymousComplaints.Add(new AnonymousComplaint { CreatedBy = 1, Description = "A" });
            context.AnonymousComplaints.Add(new AnonymousComplaint { CreatedBy = 2, Description = "B" });

            await context.SaveChangesAsync();

            var repo = CreateRepo(context);

            var result = await repo.GetByUserIdAsync(1);

            Assert.Single(result);
        }

        // ============================
        // ✅ GET NON ARCHIVED
        // ============================
        [Fact]
        public async Task GetNonArchivedAsync_ReturnsOnlyNonArchived()
        {
            var context = CreateDb();

            context.AnonymousComplaints.Add(new AnonymousComplaint { Archived = false, Description = "A" });
            context.AnonymousComplaints.Add(new AnonymousComplaint { Archived = true, Description = "B" });

            await context.SaveChangesAsync();

            var repo = CreateRepo(context);

            var result = await repo.GetNonArchivedAsync();

            Assert.Single(result);
        }

        // ============================
        // ✅ DELETE
        // ============================
        [Fact]
        public async Task DeleteAsync_RemovesEntity()
        {
            var context = CreateDb();

            context.AnonymousComplaints.Add(new AnonymousComplaint { AnonymousComplaintId = 1, Description = "Test" });
            await context.SaveChangesAsync();

            var repo = CreateRepo(context);

            await repo.DeleteAsync(1);

            Assert.Empty(context.AnonymousComplaints);
        }

        // ============================
        // ✅ UPDATE
        // ============================
        [Fact]
        public async Task UpdateAsync_UpdatesEntity()
        {
            var context = CreateDb();

            var complaint = new AnonymousComplaint
            {
                AnonymousComplaintId = 1,
                Description = "Old"
            };

            context.AnonymousComplaints.Add(complaint);
            await context.SaveChangesAsync();

            var repo = CreateRepo(context);

            complaint.Description = "Updated";

            await repo.UpdateAsync(complaint);

            var updated = await context.AnonymousComplaints.FindAsync(1);

            Assert.Equal("Updated", updated.Description);
        }

        // ============================
        // ⚠️ UPDATE STATE
        // ============================
        [Fact]
        public async Task UpdateStateAsync_ChangesState()
        {
            var context = CreateDb();

            context.AnonymousComplaints.Add(new AnonymousComplaint
            {
                AnonymousComplaintId = 1,
                State = "SUBMITTED",
                Description = "Test"
            });

            await context.SaveChangesAsync();

            var repo = CreateRepo(context);

            await repo.UpdateStateAsync(1, "RESOLVED");

            var updated = await context.AnonymousComplaints.FindAsync(1);

            // ⚠️ peut échouer avec InMemory si ExecuteUpdateAsync
            Assert.Equal("RESOLVED", updated.State);
        }

        // ============================
        // ⚠️ ARCHIVE
        // ============================
        [Fact]
        public async Task ArchiveAsync_SetsArchivedTrue()
        {
            var context = CreateDb();

            context.AnonymousComplaints.Add(new AnonymousComplaint
            {
                AnonymousComplaintId = 1,
                Archived = false,
                Description = "Test"
            });

            await context.SaveChangesAsync();

            var repo = CreateRepo(context);

            await repo.ArchiveAsync(1);

            var entity = await context.AnonymousComplaints.FindAsync(1);

            Assert.True(entity.Archived);
        }

        // ============================
        // ⚠️ RESTORE
        // ============================
        [Fact]
        public async Task RestoreAsync_SetsArchivedFalse()
        {
            var context = CreateDb();

            context.AnonymousComplaints.Add(new AnonymousComplaint
            {
                AnonymousComplaintId = 1,
                Archived = true,
                Description = "Test"
            });

            await context.SaveChangesAsync();

            var repo = CreateRepo(context);

            await repo.RestoreAsync(1);

            var entity = await context.AnonymousComplaints.FindAsync(1);

            Assert.False(entity.Archived);
        }
    }
}
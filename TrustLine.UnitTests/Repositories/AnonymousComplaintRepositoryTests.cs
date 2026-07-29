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

        private static AnonymousComplaint Complaint(string description, int? id = null, int? createdBy = null, bool? archived = false, string? state = null)
            => new AnonymousComplaint
            {
                AnonymousComplaintId = id ?? 0,
                Description = description,
                CreatedBy = createdBy,
                Archived = archived,
                State = state,
                IsIdentityVisible = false
            };

        // ============================
        // CREATE
        // ============================
        [Fact]
        public async Task CreateAsync_AddsComplaint()
        {
            var context = CreateDb();
            var repo = CreateRepo(context);

            await repo.CreateAsync(Complaint("Test"));

            Assert.Equal(1, context.AnonymousComplaints.Count());
        }

        // ============================
        // GET ALL
        // ============================
        [Fact]
        public async Task GetAllAsync_ReturnsAll()
        {
            var context = CreateDb();

            context.AnonymousComplaints.Add(Complaint("A"));
            context.AnonymousComplaints.Add(Complaint("B"));
            await context.SaveChangesAsync();

            var result = await CreateRepo(context).GetAllAsync();

            Assert.Equal(2, result.Count());
        }

        // ============================
        // GET BY ID
        // ============================
        [Fact]
        public async Task GetByIdAsync_ReturnsCorrect()
        {
            var context = CreateDb();

            context.AnonymousComplaints.Add(Complaint("Test", id: 1));
            await context.SaveChangesAsync();

            var result = await CreateRepo(context).GetByIdAsync(1);

            Assert.NotNull(result);
        }

        // ============================
        // EXISTS
        // ============================
        [Fact]
        public async Task ExistsAsync_ReturnsTrue()
        {
            var context = CreateDb();

            context.AnonymousComplaints.Add(Complaint("Test", id: 1));
            await context.SaveChangesAsync();

            var result = await CreateRepo(context).ExistsAsync(1);

            Assert.True(result);
        }

        // ============================
        // GET BY USER
        // ============================
        [Fact]
        public async Task GetByUserIdAsync_ReturnsFiltered()
        {
            var context = CreateDb();

            context.AnonymousComplaints.Add(Complaint("A", createdBy: 1));
            context.AnonymousComplaints.Add(Complaint("B", createdBy: 2));
            await context.SaveChangesAsync();

            var result = await CreateRepo(context).GetByUserIdAsync(1);

            Assert.Single(result);
        }

        // ============================
        // GET NON ARCHIVED
        // ============================
        [Fact]
        public async Task GetNonArchivedAsync_ReturnsOnlyNonArchived()
        {
            var context = CreateDb();

            context.AnonymousComplaints.Add(Complaint("A", archived: false));
            context.AnonymousComplaints.Add(Complaint("B", archived: true));
            await context.SaveChangesAsync();

            var result = await CreateRepo(context).GetNonArchivedAsync();

            Assert.Single(result);
        }

        // ============================
        // DELETE
        // ============================
        [Fact]
        public async Task DeleteAsync_RemovesEntity()
        {
            var context = CreateDb();

            context.AnonymousComplaints.Add(Complaint("Test", id: 1));
            await context.SaveChangesAsync();

            await CreateRepo(context).DeleteAsync(1);

            Assert.Empty(context.AnonymousComplaints);
        }

        // ============================
        // UPDATE
        // ============================
        [Fact]
        public async Task UpdateAsync_UpdatesEntity()
        {
            var context = CreateDb();

            var complaint = Complaint("Old", id: 1);
            context.AnonymousComplaints.Add(complaint);
            await context.SaveChangesAsync();

            complaint.Description = "Updated";
            await CreateRepo(context).UpdateAsync(complaint);

            var updated = await context.AnonymousComplaints.FindAsync(1);
            Assert.Equal("Updated", updated!.Description);
        }

        // ============================
        // UPDATE STATE
        // ============================
        [Fact]
        public async Task UpdateStateAsync_ChangesState()
        {
            var context = CreateDb();

            context.AnonymousComplaints.Add(Complaint("Test", id: 1, state: "SUBMITTED"));
            await context.SaveChangesAsync();

            await CreateRepo(context).UpdateStateAsync(1, "RESOLVED");

            var updated = await context.AnonymousComplaints.FindAsync(1);
            Assert.Equal("RESOLVED", updated!.State);
        }

        // ============================
        // ARCHIVE
        // ============================
        [Fact]
        public async Task ArchiveAsync_SetsArchivedTrue()
        {
            var context = CreateDb();

            context.AnonymousComplaints.Add(Complaint("Test", id: 1, archived: false));
            await context.SaveChangesAsync();

            await CreateRepo(context).ArchiveAsync(1);

            var entity = await context.AnonymousComplaints.FindAsync(1);
            Assert.True(entity!.Archived);
        }

        // ============================
        // RESTORE
        // ============================
        [Fact]
        public async Task RestoreAsync_SetsArchivedFalse()
        {
            var context = CreateDb();

            context.AnonymousComplaints.Add(Complaint("Test", id: 1, archived: true));
            await context.SaveChangesAsync();

            await CreateRepo(context).RestoreAsync(1);

            var entity = await context.AnonymousComplaints.FindAsync(1);
            Assert.False(entity!.Archived);
        }
    }
}

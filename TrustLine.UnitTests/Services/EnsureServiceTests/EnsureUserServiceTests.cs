using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

using AnonymousComplaintsAPI.Data;
using AnonymousComplaintsAPI.Models.Entities;
using AnonymousComplaintsAPI.Services.EnsureServices;
using AnonymousComplaintsAPI.Services.Interfaces;

namespace TrustLine.Tests.Services.EnsureServiceTests
{
    public class EnsureServiceTests
    {
        private AnonymousComplaintsV002Context CreateDb()
        {
            var options = new DbContextOptionsBuilder<AnonymousComplaintsV002Context>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AnonymousComplaintsV002Context(options);
        }

        private EnsureService CreateService(AnonymousComplaintsV002Context context)
        {
            var hrLinkMock = new Mock<IHrLinkService>();
            hrLinkMock.Setup(x => x.GetProfilesMinimalAsync())
                .ReturnsAsync(new List<AnonymousComplaintsAPI.DTOs.Responses.ShortProfileResponseDto>());

            var loggerMock = new Mock<ILogger<EnsureService>>();

            return new EnsureService(context, hrLinkMock.Object, loggerMock.Object);
        }

        [Fact]
        public async Task EnsureUserExistsAsync_UserExists_ReturnsUser()
        {
            var context = CreateDb();
            context.Users.Add(new User { UserId = 1 });
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.EnsureUserExistsAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.UserId);
        }

        [Fact]
        public async Task EnsureUserExistsAsync_UserNotExists_CreatesUser()
        {
            var context = CreateDb();
            var service = CreateService(context);

            var result = await service.EnsureUserExistsAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.UserId);

            var userInDb = await context.Users.FirstOrDefaultAsync(x => x.UserId == 1);
            Assert.NotNull(userInDb);
        }
    }
}

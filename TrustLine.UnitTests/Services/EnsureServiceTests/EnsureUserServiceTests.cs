using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AnonymousComplaintsAPI.Data;
using AnonymousComplaintsAPI.Models.Entities;
using AnonymousComplaintsAPI.Services.EnsureServices;
using AnonymousComplaintsAPI.Configurations;
using TrustLine.Tests.Mocks;

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

        private IConfiguration CreateConfig()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                { "All_Apps:0:Name", "AccessGate" },
                { "All_Apps:0:Ips:0", "http://fake/" }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        private EnsureService CreateService(AnonymousComplaintsV002Context context)
        {
            var config = CreateConfig();

            var fakeUsers = new List<object>
            {
                new { Id = 1, UserName = "testuser", Email = "test@test.com", Logged = false, Archived = false, CreatedAt = DateTime.Now }
            };

            var handler = new HttpMessageHandlerStub(
                new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(fakeUsers),
                        Encoding.UTF8,
                        "application/json"
                    )
                });

            var httpClient = new HttpClient(handler);

            var loggerMock = new Mock<ILogger<EnsureService>>();

            return new EnsureService(context, config, httpClient, loggerMock.Object);
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
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnonymousComplaintsAPI.Services.Implementations;
using AnonymousComplaintsAPI.DTOs.Responses;
using TrustLine.Tests.Mocks;

namespace TrustLine.Tests.Services
{
    public class ExternalUserServiceTests
    {
        private ExternalUserService CreateService(HttpResponseMessage response)
        {
            var handler = new HttpMessageHandlerStub(response);
            var httpClient = new HttpClient(handler);

            var logger = new Mock<ILogger<ExternalUserService>>();

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "All_Apps:0:Name", "AccessGate" },
                    { "All_Apps:0:Ips:0", "http://fake/" }
                })
                .Build();

            return new ExternalUserService(httpClient, logger.Object, config);
        }

        // =========================
        // GET USERS
        // =========================

        [Fact]
        public async Task GetUsersAsync_ShouldReturnUsers_WhenApiSuccess()
        {
            // Arrange
            var fakeUsers = new List<object>
            {
                new { Id = 1, User = new { Id = 1 } }
            };

            var json = JsonSerializer.Serialize(fakeUsers);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var service = CreateService(response);

            // Act
            var result = await service.GetUsersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result[0].UserId);
        }

        [Fact]
        public async Task GetUsersAsync_ShouldReturnEmpty_WhenApiFails()
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);

            var service = CreateService(response);

            var result = await service.GetUsersAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        // =========================
        // GET USER BY ID
        // =========================

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenSuccess()
        {
            var fakeUser = new List<object>
            {
                new { Id = 5, UserName = "test" }
            };

            var json = JsonSerializer.Serialize(fakeUser);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var service = CreateService(response);

            var result = await service.GetUserByIdAsync(5);

            Assert.NotNull(result);
            Assert.Equal(5, result!.UserId);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);

            var service = CreateService(response);

            var result = await service.GetUserByIdAsync(99);

            Assert.Null(result);
        }
    }
}
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

using AnonymousComplaintsAPI.Services.Implementations;
using AnonymousComplaintsAPI.Repositories.Interfaces;
using AnonymousComplaintsAPI.Models.Entities;

namespace TrustLine.Tests.Services
{
    public class ExternalUserServiceTests
    {
        private readonly Mock<IUserRepository> _repoMock = new();
        private readonly Mock<ILogger<ExternalUserService>> _loggerMock = new();

        private ExternalUserService CreateService() =>
            new ExternalUserService(_repoMock.Object, _loggerMock.Object);

        // =====================================================
        // GET ALL USERS
        // =====================================================
        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnUsers()
        {
            _repoMock.Setup(x => x.GetAllAsync(It.IsAny<bool>()))
                .ReturnsAsync(new List<User>
                {
                    new User { UserId = 1 },
                    new User { UserId = 2 }
                });

            var service = CreateService();
            var result = await service.GetAllUsersAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllUsersAsync_EmptyRepo_ShouldReturnEmpty()
        {
            _repoMock.Setup(x => x.GetAllAsync(It.IsAny<bool>()))
                .ReturnsAsync(new List<User>());

            var result = await CreateService().GetAllUsersAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        // =====================================================
        // GET USER BY ID
        // =====================================================
        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser()
        {
            _repoMock.Setup(x => x.GetByIdAsync(5))
                .ReturnsAsync(new User { UserId = 5 });

            var result = await CreateService().GetUserByIdAsync(5);

            Assert.NotNull(result);
            Assert.Equal(5, result!.UserId);
        }

        [Fact]
        public async Task GetUserByIdAsync_NotFound_ShouldReturnNull()
        {
            _repoMock.Setup(x => x.GetByIdAsync(99))
                .ReturnsAsync((User?)null);

            var result = await CreateService().GetUserByIdAsync(99);

            Assert.Null(result);
        }
    }
}

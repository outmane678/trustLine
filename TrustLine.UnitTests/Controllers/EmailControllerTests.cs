using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

using AnonymousComplaintsAPI.Controllers;
using AnonymousComplaintsAPI.Services.Interfaces;
using AnonymousComplaintsAPI.DTOs.Requests;

namespace TrustLine.Tests.Controllers
{
    public class EmailControllerTests
    {
        private readonly Mock<IEmailService> _emailServiceMock;

        public EmailControllerTests()
        {
            _emailServiceMock = new Mock<IEmailService>();
        }

        private EmailController CreateController()
        {
            return new EmailController(_emailServiceMock.Object);
        }

        // =========================
        // SUCCESS
        // =========================
        [Fact]
        public async Task SendEmail_ShouldReturnOk()
        {
            // Arrange
            var controller = CreateController();

            var request = new SendEmailRequest
            {
                To = "test@test.com",
                Subject = "Test",
                Body = "Hello"
            };

            _emailServiceMock
                .Setup(x => x.SendEmailAsync(It.IsAny<SendEmailRequest>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.SendEmail(request);

            // Assert
            Assert.IsType<OkResult>(result);

            _emailServiceMock.Verify(
                x => x.SendEmailAsync(It.IsAny<SendEmailRequest>()),
                Times.Once
            );
        }

        // =========================
        // EXCEPTION CASE
        // =========================
        [Fact]
        public async Task SendEmail_ShouldThrowException_WhenServiceFails()
        {
            // Arrange
            var controller = CreateController();

            var request = new SendEmailRequest
            {
                To = "test@test.com",
                Subject = "Test",
                Body = "Hello"
            };

            _emailServiceMock
                .Setup(x => x.SendEmailAsync(It.IsAny<SendEmailRequest>()))
                .ThrowsAsync(new System.Exception("SMTP error"));

            // Act & Assert
            await Assert.ThrowsAsync<System.Exception>(() => controller.SendEmail(request));
        }
    }
}
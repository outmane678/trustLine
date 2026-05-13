using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

using AnonymousComplaintsAPI.Services.Implementations;
using AnonymousComplaintsAPI.DTOs.Requests;

namespace TrustLine.Tests.Services
{
    public class EmailServiceTests
    {
        private readonly Mock<IConfiguration> _config = new();
        private readonly Mock<ILogger<EmailService>> _logger = new();

        private EmailService CreateService()
        {
            _config.Setup(x => x.GetSection("EmailUsername").Value)
                .Returns("test@test.com");

            _config.Setup(x => x.GetSection("EmailPassword").Value)
                .Returns("password");

            _config.Setup(x => x.GetSection("EmailHost").Value)
                .Returns("smtp.test.com");

            return new EmailService(_config.Object, _logger.Object);
        }

        // =========================
        // VALIDATION
        // =========================

        [Fact]
        public async Task SendEmail_ShouldThrow_WhenToIsEmpty()
        {
            var service = CreateService();

            var request = new SendEmailRequest
            {
                To = "",
                Subject = "test",
                Body = "hello"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.SendEmailAsync(request));
        }

        [Fact]
        public async Task SendEmail_ShouldThrow_WhenSubjectEmpty()
        {
            var service = CreateService();

            var request = new SendEmailRequest
            {
                To = "test@test.com",
                Subject = "",
                Body = "hello"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.SendEmailAsync(request));
        }

        // =========================
        // BODY GENERATION
        // =========================

        [Fact]
        public void GenerateComplaintBody_ShouldReturnHtml()
        {
            var service = CreateService();

            var result = service.GenerateComplaintEmailBody("test complaint");

            Assert.Contains("test complaint", result);
            Assert.Contains("<html>", result);
        }

        [Fact]
        public void GenerateSolutionBody_ShouldReturnHtml()
        {
            var service = CreateService();

            var result = service.GenerateSolutionEmailBody("solution text");

            Assert.Contains("solution text", result);
            Assert.Contains("<html>", result);
        }
    }
}
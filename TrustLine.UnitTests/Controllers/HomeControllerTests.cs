using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

using AnonymousComplaintsAPI.Controllers;
using AnonymousComplaintsAPI.DTOs.Responses;
using Microsoft.AspNetCore.Http;

namespace TrustLine.Tests.Controllers
{
    public class HomeControllerTests
    {
        private readonly Mock<ILogger<HomeController>> _loggerMock;

        public HomeControllerTests()
        {
            _loggerMock = new Mock<ILogger<HomeController>>();
        }

        private HomeController CreateController()
        {
            return new HomeController(_loggerMock.Object);
        }

        // =========================
        // INDEX
        // =========================
        [Fact]
        public void Index_ShouldReturnView()
        {
            var controller = CreateController();

            var result = controller.Index();

            Assert.IsType<ViewResult>(result);
        }

        // =========================
        // PRIVACY
        // =========================
        [Fact]
        public void Privacy_ShouldReturnView()
        {
            var controller = CreateController();

            var result = controller.Privacy();

            Assert.IsType<ViewResult>(result);
        }

        // =========================
        // ERROR
        // =========================
        [Fact]
        public void Error_ShouldReturnViewWithModel()
        {
            var controller = CreateController();
            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = controller.Error();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ErrorViewModel>(viewResult.Model);

            Assert.False(string.IsNullOrEmpty(model.RequestId));
        }
    }
}
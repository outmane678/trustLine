using System.Diagnostics;
using AnonymousComplaintsAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace AnonymousComplaintsAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur dans Index");
                return StatusCode(500, "Erreur serveur");
            }
        }

        public IActionResult Privacy()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur dans Privacy");
                return StatusCode(500, "Erreur serveur");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            try
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur dans Error");
                return StatusCode(500, "Erreur serveur");
            }
        }
    }
}

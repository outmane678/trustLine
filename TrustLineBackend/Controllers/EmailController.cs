using AnonymousComplaintsAPI.DTOs.Requests;
using AnonymousComplaintsAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AnonymousComplaintsAPI.Helpers;

namespace AnonymousComplaintsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost]
        [RequirePermission("tl-r-report")]
        public async Task<IActionResult> SendEmail(SendEmailRequest request)
        {
            try
            {
                await _emailService.SendEmailAsync(request);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de l'envoi de l'email : {ex.Message}");
            }
        }
    }
}

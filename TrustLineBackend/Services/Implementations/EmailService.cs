using AnonymousComplaintsAPI.DTOs.Requests;
using AnonymousComplaintsAPI.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace AnonymousComplaintsAPI.Services.Implementations;

/// <summary>
/// Enhanced async Email Service implementation
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(SendEmailRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.To))
            {
                throw new ArgumentException("Recipient email address is required", nameof(request.To));
            }

            if (string.IsNullOrWhiteSpace(request.Subject))
            {
                throw new ArgumentException("Email subject is required", nameof(request.Subject));
            }

            // Get email configuration
            var emailUsername = _configuration.GetSection("EmailUsername").Value;
            var emailPassword = _configuration.GetSection("EmailPassword").Value;
            var emailHost = _configuration.GetSection("EmailHost").Value;

            if (string.IsNullOrWhiteSpace(emailUsername) ||
                string.IsNullOrWhiteSpace(emailPassword) ||
                string.IsNullOrWhiteSpace(emailHost))
            {
                throw new InvalidOperationException("Email configuration is incomplete. Check EmailUsername, EmailPassword, and EmailHost settings.");
            }

            // Create email message
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(emailUsername));
            email.To.Add(MailboxAddress.Parse(request.To));
            email.Subject = request.Subject;
            email.Body = new TextPart(TextFormat.Html) { Text = request.Body };

            // Send email using SMTP
            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(emailHost, 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(emailUsername, emailPassword);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {Recipient}: {Subject}",
                request.To, request.Subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Recipient}", request.To);
            throw new InvalidOperationException($"Failed to send email to {request.To}", ex);
        }
    }

    public async Task SendComplaintNotificationAsync(int complaintId, string recipientEmail)
    {
        try
        {
            var subject = $"New Complaint Submitted - ID: {complaintId}";
            var body = GenerateComplaintEmailBody($"A new complaint (ID: {complaintId}) has been submitted and requires attention.");

            var request = new SendEmailRequest
            {
                To = recipientEmail,
                Subject = subject,
                Body = body
            };

            await SendEmailAsync(request);
            _logger.LogInformation("Complaint notification sent for complaint {ComplaintId} to {Recipient}",
                complaintId, recipientEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending complaint notification for complaint {ComplaintId}", complaintId);
            throw;
        }
    }

    public async Task SendSolutionNotificationAsync(int solutionId, string recipientEmail)
    {
        try
        {
            var subject = $"Solution Added - ID: {solutionId}";
            var body = GenerateSolutionEmailBody($"A solution (ID: {solutionId}) has been added to your complaint.");

            var request = new SendEmailRequest
            {
                To = recipientEmail,
                Subject = subject,
                Body = body
            };

            await SendEmailAsync(request);
            _logger.LogInformation("Solution notification sent for solution {SolutionId} to {Recipient}",
                solutionId, recipientEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending solution notification for solution {SolutionId}", solutionId);
            throw;
        }
    }

    public string GenerateComplaintEmailBody(string complaintDetails)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 20px; margin-top: 20px; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Anonymous Complaints System</h1>
        </div>
        <div class='content'>
            <h2>New Complaint Notification</h2>
            <p>{complaintDetails}</p>
            <p>Please log in to the system to review and process this complaint.</p>
        </div>
        <div class='footer'>
            <p>This is an automated notification from the Anonymous Complaints System.</p>
        </div>
    </div>
</body>
</html>";
    }

    public string GenerateSolutionEmailBody(string solutionDetails)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 20px; margin-top: 20px; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Anonymous Complaints System</h1>
        </div>
        <div class='content'>
            <h2>Solution Added</h2>
            <p>{solutionDetails}</p>
            <p>Your complaint has been resolved. Please log in to the system to view the solution details.</p>
        </div>
        <div class='footer'>
            <p>This is an automated notification from the Anonymous Complaints System.</p>
        </div>
    </div>
</body>
</html>";
    }
}

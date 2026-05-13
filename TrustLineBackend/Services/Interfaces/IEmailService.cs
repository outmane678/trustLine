using AnonymousComplaintsAPI.DTOs.Requests;

namespace AnonymousComplaintsAPI.Services.Interfaces;

/// <summary>
/// Service for sending emails with various templates
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email asynchronously
    /// </summary>
    /// <param name="request">Email details (recipient, subject, body)</param>
    Task SendEmailAsync(SendEmailRequest request);

    /// <summary>
    /// Sends a notification email about a new complaint
    /// </summary>
    Task SendComplaintNotificationAsync(int complaintId, string recipientEmail);

    /// <summary>
    /// Sends a notification email about a solution being added
    /// </summary>
    Task SendSolutionNotificationAsync(int solutionId, string recipientEmail);

    /// <summary>
    /// Generates HTML email body template for a complaint
    /// </summary>
    string GenerateComplaintEmailBody(string complaintDetails);

    /// <summary>
    /// Generates HTML email body template for a solution
    /// </summary>
    string GenerateSolutionEmailBody(string solutionDetails);
}

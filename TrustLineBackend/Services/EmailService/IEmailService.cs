using AnonymousComplaintsAPI.DTOs.Requests;


namespace AnonymousComplaintsAPI.Services.EmailService
{
    public interface IEmailService
    {
        void SendEmail(SendEmailRequest request);

        string BodyTemplate(SendEmailRequest mail);
    }
}

namespace Auth.Application.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetToken);
    Task SendEmailConfirmationAsync(string toEmail, string confirmationToken);
    Task SendWelcomeEmailAsync(string toEmail, string userName);
}

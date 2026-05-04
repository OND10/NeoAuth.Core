using Auth.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
    {
        // Default implementation logs the token. Integrators should override with their email provider.
        _logger.LogInformation("Password reset requested for {Email}. Token: {Token}", toEmail, resetToken);
        return Task.CompletedTask;
    }

    public Task SendEmailConfirmationAsync(string toEmail, string confirmationToken)
    {
        _logger.LogInformation("Email confirmation for {Email}. Token: {Token}", toEmail, confirmationToken);
        return Task.CompletedTask;
    }

    public Task SendWelcomeEmailAsync(string toEmail, string userName)
    {
        _logger.LogInformation("Welcome email for {Email}, user {UserName}", toEmail, userName);
        return Task.CompletedTask;
    }
}

using MailKit.Net.Smtp;
using MimeKit;

namespace CodeforcesRandomizer.Services;

public class SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger) : IEmailService
{
    public async Task SendPasswordResetEmailAsync(string email, string resetLink)
    {
        var smtpHost = configuration["Email:Host"] ?? "smtp.gmail.com";
        var smtpPort = int.Parse(configuration["Email:Port"] ?? "465");
        var smtpUser = configuration["Email:Username"];
        var smtpPass = configuration["Email:Password"];

        if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
        {
            logger.LogWarning("Email credentials not configured. Logging reset link instead.");
            logger.LogInformation("PASSWORD RESET LINK for {Email}: {Link}", email, resetLink);
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("CF Randomizer", smtpUser));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Reset Your Password";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <h2>Password Reset Request</h2>
                    <p>Click the link below to reset your password:</p>
                    <p><a href='{resetLink}'>Reset Password</a></p>
                    <p>If you didn't request this, please ignore this email.</p>
                    <br>
                    <small>This link expires in 1 hour.</small>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await client.ConnectAsync(smtpHost, smtpPort, true); // true = use SSL
            await client.AuthenticateAsync(smtpUser, smtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            logger.LogInformation("Password reset email sent to {Email}", email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email. Falling back to logging the link.");
            logger.LogWarning("PASSWORD RESET LINK for {Email}: {Link}", email, resetLink);
        }
    }
}

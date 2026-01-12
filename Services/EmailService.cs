namespace CodeforcesRandomizer.Services;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string email, string resetLink);
}

public class MockEmailService(ILogger<MockEmailService> logger) : IEmailService
{
    public Task SendPasswordResetEmailAsync(string email, string resetLink)
    {
        logger.LogInformation("Password Reset Email for {Email}: {Link}", email, resetLink);
        return Task.CompletedTask;
    }
}

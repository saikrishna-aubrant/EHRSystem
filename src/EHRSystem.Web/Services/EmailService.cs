public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string email, string resetLink);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// US-AUTH-03: Send password reset link via email
    /// </summary>
    public async Task SendPasswordResetEmailAsync(string email, string resetLink)
    {
        // Implement actual email sending logic using SMTP/SendGrid
        // This is a mock implementation for development
        
        _logger.LogInformation($"Password reset link for {email}: {resetLink}");
        
        // Uncomment for production:
        // var message = new MimeMessage();
        // message.From.Add(new MailboxAddress("EHR System", _config["Email:From"]));
        // message.To.Add(new MailboxAddress("", email));
        // message.Subject = "Password Reset Request";
        // message.Body = new TextPart("plain") { Text = $"Reset your password: {resetLink}" };
        
        // using var client = new SmtpClient();
        // await client.ConnectAsync(_config["Email:Host"], 
        //     int.Parse(_config["Email:Port"]), SecureSocketOptions.StartTls);
        // await client.AuthenticateAsync(_config["Email:User"], _config["Email:Password"]);
        // await client.SendAsync(message);
        // await client.DisconnectAsync(true);
    }
} 
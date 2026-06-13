namespace SocialChat.Application.Abstractions;

public interface IEmailSender
{
    Task SendEmailVerificationAsync(string toEmail, string username, string verificationLink, CancellationToken cancellationToken = default);
}

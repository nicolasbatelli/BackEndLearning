using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SocialChat.Application.Abstractions;

namespace SocialChat.Infrastructure.Services;

public class MailerSendOptions
{
    public const string SectionName = "MailerSend";
    public string ApiToken { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "SocialChat";
    public string BaseUrl { get; set; } = "https://api.mailersend.com/v1";
}

public class MailerSendEmailSender : IEmailSender
{
    private readonly MailerSendOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;

    public MailerSendEmailSender(IOptions<MailerSendOptions> options, IHttpClientFactory httpClientFactory)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
    }

    public async Task SendEmailVerificationAsync(
        string toEmail,
        string username,
        string verificationLink,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            from = new { email = _options.FromEmail, name = _options.FromName },
            to = new[] { new { email = toEmail, name = username } },
            subject = "Verify your SocialChat account",
            text = $"Hi {username}, verify your account by visiting: {verificationLink}",
            html = $"<p>Hi <strong>{username}</strong>,</p>" +
                   $"<p>Please verify your account by clicking <a href=\"{verificationLink}\">this link</a>.</p>"
        };

        var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.BaseUrl.TrimEnd('/')}/email")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"MailerSend failed: {(int)response.StatusCode} {response.StatusCode} - {body}");
        }
    }
}

using System.Security.Claims;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialChat.Application.Auth.Commands.GoogleSignIn;
using SocialChat.Application.Auth.Commands.Login;
using SocialChat.Application.Auth.Commands.RefreshToken;
using SocialChat.Application.Auth.Commands.RegisterUser;
using SocialChat.Application.Auth.Commands.UploadAvatar;
using SocialChat.Application.Auth.Commands.VerifyEmail;
using SocialChat.Application.Auth.Queries.GetCurrentUser;
using SocialChat.Application.Common;
using SocialChat.Infrastructure.Services;

namespace SocialChat.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AuthController(IMediator mediator, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _mediator = mediator;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<RegisterUserResult>> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return Ok(new { message = "Email verified successfully." });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        SetRefreshTokenCookie(result);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized(new { message = "Refresh token is missing." });
        }

        var result = await _mediator.Send(new RefreshTokenCommand(refreshToken), cancellationToken);
        SetRefreshTokenCookie(result);
        return Ok(result);
    }

    [Authorize(Policy = "RequireAuthenticatedUser")]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new GetCurrentUserQuery(userId), cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "RequireAuthenticatedUser")]
    [HttpPost("avatar")]
    [RequestSizeLimit(5_000_000)]
    public async Task<ActionResult<UserDto>> UploadAvatar(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "Profile picture is required." });
        }

        await using var stream = file.OpenReadStream();
        var result = await _mediator.Send(
            new UploadAvatarCommand(GetUserId(), stream, file.ContentType, file.FileName),
            cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("google")]
    public async Task<ActionResult<AuthResponse>> GoogleSignIn([FromBody] GoogleSignInRequest request, CancellationToken cancellationToken)
    {
        var googleOptions = _configuration.GetSection(GoogleAuthOptions.SectionName).Get<GoogleAuthOptions>()
            ?? throw new InvalidOperationException("Google auth is not configured.");

        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={request.IdToken}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Unauthorized(new { message = "Invalid Google token." });
        }

        var payload = await response.Content.ReadFromJsonAsync<GoogleTokenPayload>(cancellationToken: cancellationToken);
        if (payload is null || payload.Aud != googleOptions.ClientId)
        {
            return Unauthorized(new { message = "Google token audience mismatch." });
        }

        var command = new GoogleSignInCommand(
            payload.Sub,
            payload.Email,
            payload.Email.Split('@')[0],
            payload.GivenName ?? "Google",
            null,
            payload.FamilyName ?? "User",
            payload.Picture);

        var result = await _mediator.Send(command, cancellationToken);
        SetRefreshTokenCookie(result);
        return Ok(result);
    }

    [Authorize(Policy = "RequireAuthenticatedUser")]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("refreshToken");
        return Ok(new { message = "Logged out." });
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.Parse(userId!);
    }

    private void SetRefreshTokenCookie(AuthResponse response)
    {
        Response.Cookies.Append(
            "refreshToken",
            response.RefreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = Request.IsHttps ? SameSiteMode.None : SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
    }
}

public record GoogleSignInRequest(string IdToken);

public class GoogleAuthOptions
{
    public const string SectionName = "GoogleAuth";
    public string ClientId { get; set; } = string.Empty;
}

public record GoogleTokenPayload(
    string Sub,
    string Email,
    string Aud,
    string? GivenName,
    string? FamilyName,
    string? Picture);

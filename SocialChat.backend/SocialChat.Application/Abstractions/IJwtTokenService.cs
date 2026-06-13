using SocialChat.Domain.Entities;

namespace SocialChat.Application.Abstractions;

public record TokenResult(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt, DateTime RefreshTokenExpiresAt);

public interface IJwtTokenService
{
    TokenResult GenerateTokens(User user);
    Guid? GetUserIdFromExpiredToken(string accessToken);
}

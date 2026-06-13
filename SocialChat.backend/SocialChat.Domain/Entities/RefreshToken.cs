using SocialChat.Domain.Common;

namespace SocialChat.Domain.Entities;

public class RefreshToken : Entity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsRevoked { get; private set; }

    private RefreshToken()
    {
    }

    public static RefreshToken Create(Guid userId, string token, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new DomainException("Refresh token is required.");
        }

        return new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Revoke() => IsRevoked = true;

    public bool IsActive => !IsRevoked && ExpiresAt > DateTime.UtcNow;
}

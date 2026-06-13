using SocialChat.Domain.Common;
using SocialChat.Domain.ValueObjects;

namespace SocialChat.Domain.Entities;

public class User : Entity
{
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string? MiddleName { get; private set; }
    public string LastName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string? ProfilePictureUrl { get; private set; }
    public byte[]? ProfilePictureThumbnail { get; private set; }
    public string? ProfilePictureContentType { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public string? EmailVerificationToken { get; private set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; private set; }
    public string? GoogleId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public ICollection<Role> Roles { get; private set; } = new List<Role>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    private User()
    {
    }

    public static User Register(
        Username username,
        EmailAddress email,
        PersonName firstName,
        PersonName? middleName,
        PersonName lastName,
        string passwordHash,
        string emailVerificationToken,
        DateTime tokenExpiresAt,
        Role defaultRole)
    {
        return new User
        {
            Username = username.Value,
            Email = email.Value,
            FirstName = firstName.Value,
            MiddleName = middleName?.Value,
            LastName = lastName.Value,
            PasswordHash = passwordHash,
            EmailVerificationToken = emailVerificationToken,
            EmailVerificationTokenExpiresAt = tokenExpiresAt,
            IsEmailVerified = false,
            CreatedAt = DateTime.UtcNow,
            Roles = new List<Role> { defaultRole }
        };
    }

    public static User RegisterWithGoogle(
        Username username,
        EmailAddress email,
        PersonName firstName,
        PersonName? middleName,
        PersonName lastName,
        string googleId,
        string? profilePictureUrl,
        Role defaultRole)
    {
        return new User
        {
            Username = username.Value,
            Email = email.Value,
            FirstName = firstName.Value,
            MiddleName = middleName?.Value,
            LastName = lastName.Value,
            PasswordHash = string.Empty,
            GoogleId = googleId,
            ProfilePictureUrl = profilePictureUrl,
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            Roles = new List<Role> { defaultRole }
        };
    }

    public void VerifyEmail(string token)
    {
        if (IsEmailVerified)
        {
            throw new DomainException("Email is already verified.");
        }

        if (EmailVerificationToken != token)
        {
            throw new DomainException("Invalid verification token.");
        }

        if (EmailVerificationTokenExpiresAt is null || EmailVerificationTokenExpiresAt < DateTime.UtcNow)
        {
            throw new DomainException("Verification token has expired.");
        }

        IsEmailVerified = true;
        EmailVerificationToken = null;
        EmailVerificationTokenExpiresAt = null;
    }

    public void SetProfilePicture(byte[] thumbnail, string contentType)
    {
        if (thumbnail is null || thumbnail.Length == 0)
        {
            throw new DomainException("Profile picture is required.");
        }

        ProfilePictureThumbnail = thumbnail;
        ProfilePictureContentType = contentType;
        ProfilePictureUrl = null;
    }

    public void LinkGoogleAccount(string googleId, string? profilePictureUrl)
    {
        GoogleId = googleId;
        if (string.IsNullOrWhiteSpace(ProfilePictureUrl) && !string.IsNullOrWhiteSpace(profilePictureUrl))
        {
            ProfilePictureUrl = profilePictureUrl;
        }

        IsEmailVerified = true;
    }

    public string FullName => string.IsNullOrWhiteSpace(MiddleName)
        ? $"{FirstName} {LastName}"
        : $"{FirstName} {MiddleName} {LastName}";
}

using NUnit.Framework;
using SocialChat.Domain.Common;
using SocialChat.Domain.Entities;
using SocialChat.Domain.ValueObjects;

namespace SocialChat.Domain.Tests;

public class ValueObjectTests
{
    [Test]
    public void Username_Create_WithValidValue_ReturnsUsername()
    {
        // Arrange
        const string value = "user_123";

        // Act
        var username = Username.Create(value);

        // Assert
        Assert.That(username.Value, Is.EqualTo(value));
    }

    [Test]
    public void Username_Create_WithInvalidCharacters_ThrowsDomainException()
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() => Username.Create("bad-user!"));
    }

    [Test]
    public void Password_Create_WithWeakPassword_ThrowsDomainException()
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() => Password.Create("weak"));
    }

    [Test]
    public void Password_Create_WithStrongPassword_Succeeds()
    {
        // Arrange
        const string password = "StrongPass1!";

        // Act
        var result = Password.Create(password);

        // Assert
        Assert.That(result.Value, Is.EqualTo(password));
    }

    [Test]
    public void PersonName_Create_WithNumbers_ThrowsDomainException()
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() => PersonName.Create("John1", "First name"));
    }

    [Test]
    public void EmailAddress_Create_WithInvalidFormat_ThrowsDomainException()
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() => EmailAddress.Create("not-an-email"));
    }
}

public class UserTests
{
    [Test]
    public void VerifyEmail_WithValidToken_SetsEmailVerified()
    {
        // Arrange
        var role = Role.Create(Role.UserRole);
        var token = "verify-token";
        var user = User.Register(
            Username.Create("john_doe"),
            EmailAddress.Create("john@example.com"),
            PersonName.Create("John", "First name"),
            null,
            PersonName.Create("Doe", "Last name"),
            "hash",
            token,
            DateTime.UtcNow.AddHours(1),
            role);

        // Act
        user.VerifyEmail(token);

        // Assert
        Assert.That(user.IsEmailVerified, Is.True);
        Assert.That(user.EmailVerificationToken, Is.Null);
    }

    [Test]
    public void VerifyEmail_WithInvalidToken_ThrowsDomainException()
    {
        // Arrange
        var role = Role.Create(Role.UserRole);
        var user = User.Register(
            Username.Create("john_doe"),
            EmailAddress.Create("john@example.com"),
            PersonName.Create("John", "First name"),
            null,
            PersonName.Create("Doe", "Last name"),
            "hash",
            "correct-token",
            DateTime.UtcNow.AddHours(1),
            role);

        // Act & Assert
        Assert.Throws<DomainException>(() => user.VerifyEmail("wrong-token"));
    }
}

using SocialChat.Domain.Entities;

namespace SocialChat.Application.Abstractions;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}

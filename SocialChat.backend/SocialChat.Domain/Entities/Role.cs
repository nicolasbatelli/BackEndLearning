using SocialChat.Domain.Common;
using SocialChat.Domain.ValueObjects;

namespace SocialChat.Domain.Entities;

public class Role : Entity
{
    public const string UserRole = "User";
    public const string AdminRole = "Admin";

    public string Name { get; private set; } = string.Empty;
    public ICollection<User> Users { get; private set; } = new List<User>();

    private Role()
    {
    }

    public static Role Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Role name is required.");
        }

        return new Role { Name = name.Trim() };
    }
}

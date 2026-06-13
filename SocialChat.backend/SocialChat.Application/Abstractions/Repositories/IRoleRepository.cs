using SocialChat.Domain.Entities;

namespace SocialChat.Application.Abstractions.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}

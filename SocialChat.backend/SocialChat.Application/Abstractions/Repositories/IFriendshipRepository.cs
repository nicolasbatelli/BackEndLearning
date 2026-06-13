using SocialChat.Domain.Entities;
using SocialChat.Domain.Enums;

namespace SocialChat.Application.Abstractions.Repositories;

public interface IFriendshipRepository
{
    Task<Friendship?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Friendship?> GetBetweenUsersAsync(Guid userId, Guid otherUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Friendship>> GetPendingInvitesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Friendship>> GetAcceptedFriendsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Friendship friendship, CancellationToken cancellationToken = default);
}

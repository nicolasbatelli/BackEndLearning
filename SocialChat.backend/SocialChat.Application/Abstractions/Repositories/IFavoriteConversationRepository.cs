using SocialChat.Domain.Entities;

namespace SocialChat.Application.Abstractions.Repositories;

public interface IFavoriteConversationRepository
{
    Task<bool> ExistsAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FavoriteConversation>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(FavoriteConversation favorite, CancellationToken cancellationToken = default);
    Task RemoveAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken = default);
}

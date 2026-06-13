using SocialChat.Domain.Entities;
using SocialChat.Domain.Enums;

namespace SocialChat.Application.Abstractions.Repositories;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Conversation?> GetSelfConversationAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Conversation?> GetDirectConversationAsync(Guid userId, Guid otherUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Conversation>> GetUserConversationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Conversation conversation, CancellationToken cancellationToken = default);
}

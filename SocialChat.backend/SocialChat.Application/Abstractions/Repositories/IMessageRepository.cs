using SocialChat.Domain.Entities;

namespace SocialChat.Application.Abstractions.Repositories;

public interface IMessageRepository
{
    Task<IReadOnlyList<Message>> GetByConversationAsync(Guid conversationId, int take = 50, CancellationToken cancellationToken = default);
    Task AddAsync(Message message, CancellationToken cancellationToken = default);
}

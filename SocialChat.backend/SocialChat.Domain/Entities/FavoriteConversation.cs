using SocialChat.Domain.Common;

namespace SocialChat.Domain.Entities;

public class FavoriteConversation : Entity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public Guid ConversationId { get; private set; }
    public Conversation Conversation { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    private FavoriteConversation()
    {
    }

    public static FavoriteConversation Create(Guid userId, Guid conversationId)
    {
        return new FavoriteConversation
        {
            UserId = userId,
            ConversationId = conversationId,
            CreatedAt = DateTime.UtcNow
        };
    }
}

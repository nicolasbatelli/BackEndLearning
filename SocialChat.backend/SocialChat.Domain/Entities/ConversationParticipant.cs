using SocialChat.Domain.Common;

namespace SocialChat.Domain.Entities;

public class ConversationParticipant : Entity
{
    public Guid ConversationId { get; private set; }
    public Conversation Conversation { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public DateTime JoinedAt { get; private set; }

    private ConversationParticipant()
    {
    }

    public static ConversationParticipant Create(Guid conversationId, Guid userId)
    {
        return new ConversationParticipant
        {
            ConversationId = conversationId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        };
    }
}

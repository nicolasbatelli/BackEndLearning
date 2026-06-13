using SocialChat.Domain.Common;

namespace SocialChat.Domain.Entities;

public class Message : Entity
{
    public Guid ConversationId { get; private set; }
    public Conversation Conversation { get; private set; } = null!;
    public Guid SenderId { get; private set; }
    public User Sender { get; private set; } = null!;
    public string Content { get; private set; } = string.Empty;
    public DateTime SentAt { get; private set; }

    private Message()
    {
    }

    public static Message Create(Guid conversationId, Guid senderId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new DomainException("Message content is required.");
        }

        if (content.Length > 4000)
        {
            throw new DomainException("Message content cannot exceed 4000 characters.");
        }

        return new Message
        {
            ConversationId = conversationId,
            SenderId = senderId,
            Content = content.Trim(),
            SentAt = DateTime.UtcNow
        };
    }
}

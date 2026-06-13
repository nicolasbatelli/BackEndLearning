using SocialChat.Domain.Common;
using SocialChat.Domain.Enums;

namespace SocialChat.Domain.Entities;

public class Notification : Entity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }
    public Guid? RelatedEntityId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Notification()
    {
    }

    public static Notification Create(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        Guid? relatedEntityId = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Notification title is required.");
        }

        return new Notification
        {
            UserId = userId,
            Type = type,
            Title = title.Trim(),
            Message = message.Trim(),
            RelatedEntityId = relatedEntityId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsRead() => IsRead = true;
}

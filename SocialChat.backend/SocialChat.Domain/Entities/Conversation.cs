using SocialChat.Domain.Common;
using SocialChat.Domain.Enums;

namespace SocialChat.Domain.Entities;

public class Conversation : Entity
{
    public ConversationType Type { get; private set; }
    public string? Name { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public ICollection<ConversationParticipant> Participants { get; private set; } = new List<ConversationParticipant>();
    public ICollection<Message> Messages { get; private set; } = new List<Message>();

    private Conversation()
    {
    }

    public static Conversation CreateSelf(Guid userId)
    {
        var conversation = new Conversation
        {
            Type = ConversationType.Self,
            Name = "Saved Messages",
            CreatedAt = DateTime.UtcNow
        };

        conversation.Participants.Add(ConversationParticipant.Create(conversation.Id, userId));
        return conversation;
    }

    public static Conversation CreateDirect(Guid userId, Guid otherUserId)
    {
        var conversation = new Conversation
        {
            Type = ConversationType.Direct,
            CreatedAt = DateTime.UtcNow
        };

        conversation.Participants.Add(ConversationParticipant.Create(conversation.Id, userId));
        conversation.Participants.Add(ConversationParticipant.Create(conversation.Id, otherUserId));
        return conversation;
    }

    public static Conversation CreateGroup(string name, Guid creatorId, IEnumerable<Guid> memberIds)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Group name is required.");
        }

        var conversation = new Conversation
        {
            Type = ConversationType.Group,
            Name = name.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        conversation.Participants.Add(ConversationParticipant.Create(conversation.Id, creatorId));
        foreach (var memberId in memberIds.Distinct().Where(id => id != creatorId))
        {
            conversation.Participants.Add(ConversationParticipant.Create(conversation.Id, memberId));
        }

        return conversation;
    }

    public bool HasParticipant(Guid userId) =>
        Participants.Any(p => p.UserId == userId);
}

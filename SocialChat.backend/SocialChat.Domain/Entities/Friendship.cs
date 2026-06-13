using SocialChat.Domain.Common;
using SocialChat.Domain.Enums;

namespace SocialChat.Domain.Entities;

public class Friendship : Entity
{
    public Guid RequesterId { get; private set; }
    public User Requester { get; private set; } = null!;
    public Guid AddresseeId { get; private set; }
    public User Addressee { get; private set; } = null!;
    public FriendshipStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RespondedAt { get; private set; }

    private Friendship()
    {
    }

    public static Friendship CreateInvite(Guid requesterId, Guid addresseeId)
    {
        if (requesterId == addresseeId)
        {
            throw new DomainException("You cannot invite yourself.");
        }

        return new Friendship
        {
            RequesterId = requesterId,
            AddresseeId = addresseeId,
            Status = FriendshipStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Accept()
    {
        if (Status != FriendshipStatus.Pending)
        {
            throw new DomainException("Only pending invites can be accepted.");
        }

        Status = FriendshipStatus.Accepted;
        RespondedAt = DateTime.UtcNow;
    }

    public void Decline()
    {
        if (Status != FriendshipStatus.Pending)
        {
            throw new DomainException("Only pending invites can be declined.");
        }

        Status = FriendshipStatus.Declined;
        RespondedAt = DateTime.UtcNow;
    }
}

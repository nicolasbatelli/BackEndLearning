using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialChat.Domain.Entities;

namespace SocialChat.Infrastructure.Persistence.Configurations;

public class FriendshipConfiguration : IEntityTypeConfiguration<Friendship>
{
    public void Configure(EntityTypeBuilder<Friendship> builder)
    {
        builder.ToTable("Friendships");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.RequesterId, x.AddresseeId }).IsUnique();
        builder.HasOne(x => x.Requester).WithMany().HasForeignKey(x => x.RequesterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Addressee).WithMany().HasForeignKey(x => x.AddresseeId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(1000).IsRequired();
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
    }
}

public class FavoriteConversationConfiguration : IEntityTypeConfiguration<FavoriteConversation>
{
    public void Configure(EntityTypeBuilder<FavoriteConversation> builder)
    {
        builder.ToTable("FavoriteConversations");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.UserId, x.ConversationId }).IsUnique();
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        builder.HasOne(x => x.Conversation).WithMany().HasForeignKey(x => x.ConversationId);
    }
}

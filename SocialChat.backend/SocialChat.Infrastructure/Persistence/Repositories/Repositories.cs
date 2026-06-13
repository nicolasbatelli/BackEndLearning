using Microsoft.EntityFrameworkCore;
using SocialChat.Application.Abstractions.Repositories;
using SocialChat.Domain.Entities;
using SocialChat.Domain.Enums;

namespace SocialChat.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        await _context.Users.AddAsync(user, cancellationToken);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        await _context.Users.AnyAsync(u => u.Email == email.ToLower(), cancellationToken);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Email == email.ToLower(), cancellationToken);

    public async Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default) =>
        await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.GoogleId == googleId, cancellationToken);

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

    public async Task<IReadOnlyList<User>> SearchByUsernameAsync(string query, Guid excludeUserId, int take = 20, CancellationToken cancellationToken = default) =>
        await _context.Users
            .Where(u => u.Id != excludeUserId && u.Username.Contains(query))
            .OrderBy(u => u.Username)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default) =>
        await _context.Users.AnyAsync(u => u.Username == username, cancellationToken);
}

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _context;

    public RoleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        await _context.Roles.FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
}

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default) =>
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default) =>
        await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
}

public class ConversationRepository : IConversationRepository
{
    private readonly AppDbContext _context;

    public ConversationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Conversation conversation, CancellationToken cancellationToken = default) =>
        await _context.Conversations.AddAsync(conversation, cancellationToken);

    public async Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Conversations
            .Include(c => c.Participants)
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<Conversation?> GetDirectConversationAsync(Guid userId, Guid otherUserId, CancellationToken cancellationToken = default) =>
        await _context.Conversations
            .Include(c => c.Participants)
            .Include(c => c.Messages)
            .Where(c => c.Type == ConversationType.Direct)
            .Where(c => c.Participants.Any(p => p.UserId == userId) && c.Participants.Any(p => p.UserId == otherUserId))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Conversation?> GetSelfConversationAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _context.Conversations
            .Include(c => c.Participants)
            .Include(c => c.Messages)
            .Where(c => c.Type == ConversationType.Self)
            .Where(c => c.Participants.Any(p => p.UserId == userId))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<Conversation>> GetUserConversationsAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _context.Conversations
            .Include(c => c.Participants)
            .Include(c => c.Messages)
            .Where(c => c.Participants.Any(p => p.UserId == userId))
            .OrderByDescending(c => c.Messages.Max(m => (DateTime?)m.SentAt) ?? c.CreatedAt)
            .ToListAsync(cancellationToken);
}

public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _context;

    public MessageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Message message, CancellationToken cancellationToken = default) =>
        await _context.Messages.AddAsync(message, cancellationToken);

    public async Task<IReadOnlyList<Message>> GetByConversationAsync(Guid conversationId, int take = 50, CancellationToken cancellationToken = default) =>
        await _context.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.SentAt)
            .Take(take)
            .OrderBy(m => m.SentAt)
            .ToListAsync(cancellationToken);
}

public class FriendshipRepository : IFriendshipRepository
{
    private readonly AppDbContext _context;

    public FriendshipRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Friendship friendship, CancellationToken cancellationToken = default) =>
        await _context.Friendships.AddAsync(friendship, cancellationToken);

    public async Task<Friendship?> GetBetweenUsersAsync(Guid userId, Guid otherUserId, CancellationToken cancellationToken = default) =>
        await _context.Friendships.FirstOrDefaultAsync(
            f => f.RequesterId == userId && f.AddresseeId == otherUserId,
            cancellationToken);

    public async Task<Friendship?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Friendships.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Friendship>> GetAcceptedFriendsAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _context.Friendships
            .Where(f => f.Status == FriendshipStatus.Accepted && (f.RequesterId == userId || f.AddresseeId == userId))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Friendship>> GetPendingInvitesAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _context.Friendships
            .Where(f => f.Status == FriendshipStatus.Pending && f.AddresseeId == userId)
            .ToListAsync(cancellationToken);
}

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default) =>
        await _context.Notifications.AddAsync(notification, cancellationToken);

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Notification>> GetByUserAsync(Guid userId, int take = 50, CancellationToken cancellationToken = default) =>
        await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
}

public class FavoriteConversationRepository : IFavoriteConversationRepository
{
    private readonly AppDbContext _context;

    public FavoriteConversationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(FavoriteConversation favorite, CancellationToken cancellationToken = default) =>
        await _context.FavoriteConversations.AddAsync(favorite, cancellationToken);

    public async Task<bool> ExistsAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken = default) =>
        await _context.FavoriteConversations.AnyAsync(f => f.UserId == userId && f.ConversationId == conversationId, cancellationToken);

    public async Task<IReadOnlyList<FavoriteConversation>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _context.FavoriteConversations.Where(f => f.UserId == userId).ToListAsync(cancellationToken);

    public async Task RemoveAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken = default)
    {
        var favorite = await _context.FavoriteConversations
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ConversationId == conversationId, cancellationToken);
        if (favorite is not null)
        {
            _context.FavoriteConversations.Remove(favorite);
        }
    }
}

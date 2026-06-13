using Microsoft.AspNetCore.SignalR;
using SocialChat.Api.Hubs;
using SocialChat.Application.Abstractions;
using SocialChat.Application.Common;

namespace SocialChat.Api.Services;

public class RealtimeNotifier : IRealtimeNotifier
{
    private readonly IHubContext<ChatHub> _chatHub;
    private readonly IHubContext<NotificationHub> _notificationHub;

    public RealtimeNotifier(IHubContext<ChatHub> chatHub, IHubContext<NotificationHub> notificationHub)
    {
        _chatHub = chatHub;
        _notificationHub = notificationHub;
    }

    public Task PushMessageAsync(MessageDto message, CancellationToken cancellationToken = default) =>
        _chatHub.Clients.Group(message.ConversationId.ToString()).SendAsync("ReceiveMessage", message, cancellationToken);

    public Task PushNotificationAsync(Guid userId, NotificationDto notification, CancellationToken cancellationToken = default) =>
        _notificationHub.Clients.Group(userId.ToString()).SendAsync("ReceiveNotification", notification, cancellationToken);
}

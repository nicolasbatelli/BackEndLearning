using SocialChat.Application.Common;

namespace SocialChat.Application.Abstractions;

public interface IRealtimeNotifier
{
    Task PushMessageAsync(MessageDto message, CancellationToken cancellationToken = default);
    Task PushNotificationAsync(Guid userId, NotificationDto notification, CancellationToken cancellationToken = default);
}

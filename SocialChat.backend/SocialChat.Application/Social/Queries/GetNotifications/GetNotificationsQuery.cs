using MediatR;
using SocialChat.Application.Abstractions.Repositories;
using SocialChat.Application.Common;

namespace SocialChat.Application.Social.Queries.GetNotifications;

public record GetNotificationsQuery(Guid UserId) : IRequest<IReadOnlyList<NotificationDto>>;

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, IReadOnlyList<NotificationDto>>
{
    private readonly INotificationRepository _notificationRepository;

    public GetNotificationsQueryHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<IReadOnlyList<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = await _notificationRepository.GetByUserAsync(request.UserId, cancellationToken: cancellationToken);
        return notifications
            .Select(n => new NotificationDto(
                n.Id,
                n.Type.ToString(),
                n.Title,
                n.Message,
                n.IsRead,
                n.RelatedEntityId,
                n.CreatedAt))
            .ToList();
    }
}

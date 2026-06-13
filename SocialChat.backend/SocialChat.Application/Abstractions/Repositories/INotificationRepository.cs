using SocialChat.Domain.Entities;

namespace SocialChat.Application.Abstractions.Repositories;

public interface INotificationRepository
{
    Task<IReadOnlyList<Notification>> GetByUserAsync(Guid userId, int take = 50, CancellationToken cancellationToken = default);
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
}

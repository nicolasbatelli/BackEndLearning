using FluentValidation;
using MediatR;
using SocialChat.Application.Abstractions;
using SocialChat.Application.Abstractions.Repositories;
using SocialChat.Application.Common;
using SocialChat.Domain.Entities;
using SocialChat.Domain.Enums;

namespace SocialChat.Application.Social.Commands.SendFriendInvite;

public record SendFriendInviteCommand(Guid RequesterId, Guid AddresseeId) : IRequest<FriendshipDto>;

public class SendFriendInviteCommandValidator : AbstractValidator<SendFriendInviteCommand>
{
    public SendFriendInviteCommandValidator()
    {
        RuleFor(x => x.AddresseeId).NotEmpty();
    }
}

public class SendFriendInviteCommandHandler : IRequestHandler<SendFriendInviteCommand, FriendshipDto>
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IRealtimeNotifier _realtimeNotifier;
    private readonly IUnitOfWork _unitOfWork;

    public SendFriendInviteCommandHandler(
        IFriendshipRepository friendshipRepository,
        IUserRepository userRepository,
        INotificationRepository notificationRepository,
        IRealtimeNotifier realtimeNotifier,
        IUnitOfWork unitOfWork)
    {
        _friendshipRepository = friendshipRepository;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
        _realtimeNotifier = realtimeNotifier;
        _unitOfWork = unitOfWork;
    }

    public async Task<FriendshipDto> Handle(SendFriendInviteCommand request, CancellationToken cancellationToken)
    {
        if (request.RequesterId == request.AddresseeId)
        {
            throw new BusinessRuleException("You cannot invite yourself.");
        }

        var existing = await _friendshipRepository.GetBetweenUsersAsync(request.RequesterId, request.AddresseeId, cancellationToken);
        if (existing is not null)
        {
            throw new BusinessRuleException("A friendship or invite already exists.");
        }

        var requester = await _userRepository.GetByIdAsync(request.RequesterId, cancellationToken)
            ?? throw new NotFoundException("Requester not found.");
        var addressee = await _userRepository.GetByIdAsync(request.AddresseeId, cancellationToken)
            ?? throw new NotFoundException("Addressee not found.");

        var friendship = Friendship.CreateInvite(request.RequesterId, request.AddresseeId);
        await _friendshipRepository.AddAsync(friendship, cancellationToken);
        var notification = Notification.Create(
            addressee.Id,
            NotificationType.FriendInvite,
            "Friend invite",
            $"{requester.Username} sent you a friend invite.",
            friendship.Id);
        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var notificationDto = new NotificationDto(
            notification.Id,
            notification.Type.ToString(),
            notification.Title,
            notification.Message,
            notification.IsRead,
            notification.RelatedEntityId,
            notification.CreatedAt);
        await _realtimeNotifier.PushNotificationAsync(addressee.Id, notificationDto, cancellationToken);

        return new FriendshipDto(
            friendship.Id,
            friendship.RequesterId,
            requester.Username,
            friendship.AddresseeId,
            addressee.Username,
            friendship.Status.ToString(),
            friendship.CreatedAt);
    }
}

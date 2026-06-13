using FluentValidation;
using MediatR;
using SocialChat.Application.Abstractions;
using SocialChat.Application.Abstractions.Repositories;
using SocialChat.Application.Common;
using SocialChat.Domain.Enums;

namespace SocialChat.Application.Social.Commands.RespondFriendInvite;

public record RespondFriendInviteCommand(Guid UserId, Guid FriendshipId, bool Accept) : IRequest<FriendshipDto>;

public class RespondFriendInviteCommandValidator : AbstractValidator<RespondFriendInviteCommand>
{
    public RespondFriendInviteCommandValidator()
    {
        RuleFor(x => x.FriendshipId).NotEmpty();
    }
}

public class RespondFriendInviteCommandHandler : IRequestHandler<RespondFriendInviteCommand, FriendshipDto>
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RespondFriendInviteCommandHandler(
        IFriendshipRepository friendshipRepository,
        IUserRepository userRepository,
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _friendshipRepository = friendshipRepository;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FriendshipDto> Handle(RespondFriendInviteCommand request, CancellationToken cancellationToken)
    {
        var friendship = await _friendshipRepository.GetByIdAsync(request.FriendshipId, cancellationToken)
            ?? throw new NotFoundException("Invite not found.");

        if (friendship.AddresseeId != request.UserId)
        {
            throw new BusinessRuleException("You can only respond to invites sent to you.");
        }

        if (request.Accept)
        {
            friendship.Accept();
            var addressee = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
                ?? throw new NotFoundException("User not found.");
            await _notificationRepository.AddAsync(
                Domain.Entities.Notification.Create(
                    friendship.RequesterId,
                    NotificationType.FriendInviteAccepted,
                    "Invite accepted",
                    $"{addressee.Username} accepted your friend invite.",
                    friendship.Id),
                cancellationToken);
        }
        else
        {
            friendship.Decline();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var requester = await _userRepository.GetByIdAsync(friendship.RequesterId, cancellationToken)
            ?? throw new NotFoundException("Requester not found.");
        var addresseeUser = await _userRepository.GetByIdAsync(friendship.AddresseeId, cancellationToken)
            ?? throw new NotFoundException("Addressee not found.");

        return new FriendshipDto(
            friendship.Id,
            friendship.RequesterId,
            requester.Username,
            friendship.AddresseeId,
            addresseeUser.Username,
            friendship.Status.ToString(),
            friendship.CreatedAt);
    }
}

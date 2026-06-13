using MediatR;
using SocialChat.Application.Abstractions.Repositories;
using SocialChat.Application.Common;

namespace SocialChat.Application.Social.Queries.GetFriends;

public record GetFriendsQuery(Guid UserId) : IRequest<IReadOnlyList<FriendDto>>;

public class GetFriendsQueryHandler : IRequestHandler<GetFriendsQuery, IReadOnlyList<FriendDto>>
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IUserRepository _userRepository;

    public GetFriendsQueryHandler(IFriendshipRepository friendshipRepository, IUserRepository userRepository)
    {
        _friendshipRepository = friendshipRepository;
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<FriendDto>> Handle(GetFriendsQuery request, CancellationToken cancellationToken)
    {
        var friendships = await _friendshipRepository.GetAcceptedFriendsAsync(request.UserId, cancellationToken);
        var result = new List<FriendDto>();

        foreach (var friendship in friendships)
        {
            var friendId = friendship.RequesterId == request.UserId
                ? friendship.AddresseeId
                : friendship.RequesterId;

            var friend = await _userRepository.GetByIdAsync(friendId, cancellationToken);
            if (friend is not null)
            {
                result.Add(new FriendDto(
                    friend.Id,
                    friend.Username,
                    friend.FullName,
                    friend.ResolveProfilePicture()));
            }
        }

        return result;
    }
}

using MediatR;
using SocialChat.Application.Abstractions.Repositories;
using SocialChat.Application.Common;
using SocialChat.Domain.Enums;

namespace SocialChat.Application.Social.Queries.SearchUsers;

public record SearchUsersQuery(Guid UserId, string Query) : IRequest<IReadOnlyList<UserSearchResultDto>>;

public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, IReadOnlyList<UserSearchResultDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IFriendshipRepository _friendshipRepository;

    public SearchUsersQueryHandler(IUserRepository userRepository, IFriendshipRepository friendshipRepository)
    {
        _userRepository = userRepository;
        _friendshipRepository = friendshipRepository;
    }

    public async Task<IReadOnlyList<UserSearchResultDto>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.SearchByUsernameAsync(request.Query, request.UserId, cancellationToken: cancellationToken);
        var result = new List<UserSearchResultDto>();

        foreach (var user in users)
        {
            var friendship = await _friendshipRepository.GetBetweenUsersAsync(request.UserId, user.Id, cancellationToken)
                ?? await _friendshipRepository.GetBetweenUsersAsync(user.Id, request.UserId, cancellationToken);

            var status = friendship?.Status.ToString() ?? "None";
            result.Add(new UserSearchResultDto(
                user.Id,
                user.Username,
                user.FullName,
                user.ProfilePictureUrl,
                status));
        }

        return result;
    }
}

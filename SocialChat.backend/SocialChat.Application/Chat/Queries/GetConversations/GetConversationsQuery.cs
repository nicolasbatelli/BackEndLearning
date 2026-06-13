using MediatR;
using SocialChat.Application.Abstractions.Repositories;
using SocialChat.Application.Common;

namespace SocialChat.Application.Chat.Queries.GetConversations;

public record GetConversationsQuery(Guid UserId) : IRequest<IReadOnlyList<ConversationDto>>;

public class GetConversationsQueryHandler : IRequestHandler<GetConversationsQuery, IReadOnlyList<ConversationDto>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFavoriteConversationRepository _favoriteRepository;

    public GetConversationsQueryHandler(
        IConversationRepository conversationRepository,
        IUserRepository userRepository,
        IFavoriteConversationRepository favoriteRepository)
    {
        _conversationRepository = conversationRepository;
        _userRepository = userRepository;
        _favoriteRepository = favoriteRepository;
    }

    public async Task<IReadOnlyList<ConversationDto>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
    {
        var conversations = await _conversationRepository.GetUserConversationsAsync(request.UserId, cancellationToken);
        var favorites = (await _favoriteRepository.GetByUserAsync(request.UserId, cancellationToken))
            .Select(f => f.ConversationId)
            .ToHashSet();

        var result = new List<ConversationDto>();
        foreach (var conversation in conversations)
        {
            var participants = new List<ConversationParticipantDto>();
            foreach (var participant in conversation.Participants)
            {
                var user = await _userRepository.GetByIdAsync(participant.UserId, cancellationToken);
                if (user is not null)
                {
                    participants.Add(new ConversationParticipantDto(
                        user.Id,
                        user.Username,
                        user.FullName,
                        user.ProfilePictureUrl));
                }
            }

            var lastMessage = conversation.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();
            result.Add(new ConversationDto(
                conversation.Id,
                conversation.Type.ToString(),
                conversation.Name,
                lastMessage?.Content,
                lastMessage?.SentAt,
                favorites.Contains(conversation.Id),
                participants));
        }

        return result;
    }
}

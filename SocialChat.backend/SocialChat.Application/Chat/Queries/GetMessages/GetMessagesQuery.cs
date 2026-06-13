using MediatR;
using SocialChat.Application.Abstractions.Repositories;
using SocialChat.Application.Common;

namespace SocialChat.Application.Chat.Queries.GetMessages;

public record GetMessagesQuery(Guid UserId, Guid ConversationId) : IRequest<IReadOnlyList<MessageDto>>;

public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, IReadOnlyList<MessageDto>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;

    public GetMessagesQueryHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IUserRepository userRepository)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<MessageDto>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken)
            ?? throw new NotFoundException("Conversation not found.");

        if (!conversation.HasParticipant(request.UserId))
        {
            throw new BusinessRuleException("You are not a participant in this conversation.");
        }

        var messages = await _messageRepository.GetByConversationAsync(request.ConversationId, cancellationToken: cancellationToken);
        var result = new List<MessageDto>();

        foreach (var message in messages)
        {
            var sender = await _userRepository.GetByIdAsync(message.SenderId, cancellationToken);
            result.Add(new MessageDto(
                message.Id,
                message.ConversationId,
                message.SenderId,
                sender?.Username ?? "Unknown",
                message.Content,
                message.SentAt));
        }

        return result;
    }
}

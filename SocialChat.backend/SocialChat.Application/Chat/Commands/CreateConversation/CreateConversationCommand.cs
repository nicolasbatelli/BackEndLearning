using FluentValidation;
using MediatR;
using SocialChat.Application.Abstractions;
using SocialChat.Application.Abstractions.Repositories;
using SocialChat.Application.Common;
using SocialChat.Domain.Entities;
using SocialChat.Domain.Enums;

namespace SocialChat.Application.Chat.Commands.CreateConversation;

public record CreateConversationCommand(
    Guid UserId,
    string Type,
    string? Name,
    IReadOnlyList<Guid>? ParticipantIds) : IRequest<ConversationDto>;

public class CreateConversationCommandValidator : AbstractValidator<CreateConversationCommand>
{
    public CreateConversationCommandValidator()
    {
        RuleFor(x => x.Type).NotEmpty();
    }
}

public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, ConversationDto>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFavoriteConversationRepository _favoriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateConversationCommandHandler(
        IConversationRepository conversationRepository,
        IUserRepository userRepository,
        IFavoriteConversationRepository favoriteRepository,
        IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _userRepository = userRepository;
        _favoriteRepository = favoriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ConversationDto> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        Conversation conversation;
        if (request.Type.Equals("Direct", StringComparison.OrdinalIgnoreCase))
        {
            var otherUserId = request.ParticipantIds?.FirstOrDefault()
                ?? throw new BusinessRuleException("Direct conversation requires one participant.");

            var existing = await _conversationRepository.GetDirectConversationAsync(request.UserId, otherUserId, cancellationToken);
            if (existing is not null)
            {
                return await MapConversation(existing, request.UserId, cancellationToken);
            }

            conversation = Conversation.CreateDirect(request.UserId, otherUserId);
        }
        else if (request.Type.Equals("Group", StringComparison.OrdinalIgnoreCase))
        {
            conversation = Conversation.CreateGroup(
                request.Name ?? throw new BusinessRuleException("Group name is required."),
                request.UserId,
                request.ParticipantIds ?? Array.Empty<Guid>());
        }
        else
        {
            throw new BusinessRuleException("Unsupported conversation type.");
        }

        await _conversationRepository.AddAsync(conversation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await MapConversation(conversation, request.UserId, cancellationToken);
    }

    private async Task<ConversationDto> MapConversation(Conversation conversation, Guid userId, CancellationToken cancellationToken)
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

        var isFavorite = await _favoriteRepository.ExistsAsync(userId, conversation.Id, cancellationToken);
        var lastMessage = conversation.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();

        return new ConversationDto(
            conversation.Id,
            conversation.Type.ToString(),
            conversation.Name,
            lastMessage?.Content,
            lastMessage?.SentAt,
            isFavorite,
            participants);
    }
}

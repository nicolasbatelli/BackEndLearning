using FluentValidation;
using MediatR;
using SocialChat.Application.Abstractions;
using SocialChat.Application.Abstractions.Repositories;
using SocialChat.Application.Common;
using SocialChat.Domain.Entities;
using SocialChat.Domain.Enums;

namespace SocialChat.Application.Chat.Commands.SendMessage;

public record SendMessageCommand(Guid UserId, Guid ConversationId, string Content) : IRequest<MessageDto>;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.Content).NotEmpty().MaximumLength(4000);
    }
}

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, MessageDto>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IRealtimeNotifier _realtimeNotifier;
    private readonly IUnitOfWork _unitOfWork;

    public SendMessageCommandHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IUserRepository userRepository,
        INotificationRepository notificationRepository,
        IRealtimeNotifier realtimeNotifier,
        IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
        _realtimeNotifier = realtimeNotifier;
        _unitOfWork = unitOfWork;
    }

    public async Task<MessageDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken)
            ?? throw new NotFoundException("Conversation not found.");

        if (!conversation.HasParticipant(request.UserId))
        {
            throw new BusinessRuleException("You are not a participant in this conversation.");
        }

        var sender = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var message = Message.Create(request.ConversationId, request.UserId, request.Content);
        await _messageRepository.AddAsync(message, cancellationToken);

        foreach (var participant in conversation.Participants.Where(p => p.UserId != request.UserId))
        {
            var notification = Notification.Create(
                participant.UserId,
                NotificationType.Message,
                "New message",
                $"{sender.Username}: {message.Content}",
                message.Id);
            await _notificationRepository.AddAsync(notification, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var messageDto = new MessageDto(
            message.Id,
            message.ConversationId,
            message.SenderId,
            sender.Username,
            message.Content,
            message.SentAt);

        await _realtimeNotifier.PushMessageAsync(messageDto, cancellationToken);

        return messageDto;
    }
}

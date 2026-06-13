using FluentValidation;
using MediatR;
using SocialChat.Application.Abstractions;
using SocialChat.Application.Abstractions.Repositories;
using SocialChat.Application.Common;
using SocialChat.Domain.Entities;

namespace SocialChat.Application.Social.Commands.ToggleFavorite;

public record ToggleFavoriteCommand(Guid UserId, Guid ConversationId, bool IsFavorite) : IRequest<Unit>;

public class ToggleFavoriteCommandValidator : AbstractValidator<ToggleFavoriteCommand>
{
    public ToggleFavoriteCommandValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
    }
}

public class ToggleFavoriteCommandHandler : IRequestHandler<ToggleFavoriteCommand, Unit>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IFavoriteConversationRepository _favoriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ToggleFavoriteCommandHandler(
        IConversationRepository conversationRepository,
        IFavoriteConversationRepository favoriteRepository,
        IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _favoriteRepository = favoriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(ToggleFavoriteCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken)
            ?? throw new NotFoundException("Conversation not found.");

        if (!conversation.HasParticipant(request.UserId))
        {
            throw new BusinessRuleException("You are not a participant in this conversation.");
        }

        var exists = await _favoriteRepository.ExistsAsync(request.UserId, request.ConversationId, cancellationToken);
        if (request.IsFavorite && !exists)
        {
            await _favoriteRepository.AddAsync(
                FavoriteConversation.Create(request.UserId, request.ConversationId),
                cancellationToken);
        }
        else if (!request.IsFavorite && exists)
        {
            await _favoriteRepository.RemoveAsync(request.UserId, request.ConversationId, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

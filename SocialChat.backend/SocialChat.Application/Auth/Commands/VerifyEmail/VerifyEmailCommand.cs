using FluentValidation;
using MediatR;
using SocialChat.Application.Abstractions;
using SocialChat.Application.Abstractions.Repositories;
using SocialChat.Application.Common;
using SocialChat.Domain.Entities;

namespace SocialChat.Application.Auth.Commands.VerifyEmail;

public record VerifyEmailCommand(string Email, string Token) : IRequest<Unit>;

public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Token).NotEmpty();
    }
}

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyEmailCommandHandler(
        IUserRepository userRepository,
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        user.VerifyEmail(request.Token);

        var selfConversation = await _conversationRepository.GetSelfConversationAsync(user.Id, cancellationToken);
        if (selfConversation is null)
        {
            await _conversationRepository.AddAsync(Conversation.CreateSelf(user.Id), cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

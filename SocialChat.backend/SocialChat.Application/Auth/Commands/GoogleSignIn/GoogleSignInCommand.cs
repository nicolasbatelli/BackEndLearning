using FluentValidation;
using MediatR;
using SocialChat.Application.Abstractions;
using SocialChat.Application.Abstractions.Repositories;
using SocialChat.Application.Common;
using SocialChat.Domain.Entities;
using SocialChat.Domain.ValueObjects;

namespace SocialChat.Application.Auth.Commands.GoogleSignIn;

public record GoogleSignInCommand(
    string GoogleId,
    string Email,
    string Username,
    string FirstName,
    string? MiddleName,
    string LastName,
    string? ProfilePictureUrl) : IRequest<AuthResponse>;

public class GoogleSignInCommandValidator : AbstractValidator<GoogleSignInCommand>
{
    public GoogleSignInCommandValidator()
    {
        RuleFor(x => x.GoogleId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
    }
}

public class GoogleSignInCommandHandler : IRequestHandler<GoogleSignInCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GoogleSignInCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenService jwtTokenService,
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenService = jwtTokenService;
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponse> Handle(GoogleSignInCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByGoogleIdAsync(request.GoogleId, cancellationToken);

        if (user is null)
        {
            user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user is not null)
            {
                user.LinkGoogleAccount(request.GoogleId, request.ProfilePictureUrl);
            }
        }

        if (user is null)
        {
            var defaultRole = await _roleRepository.GetByNameAsync(Role.UserRole, cancellationToken)
                ?? throw new BusinessRuleException("Default role is not configured.");

            var username = await ResolveUniqueUsernameAsync(request.Username, cancellationToken);
            user = User.RegisterWithGoogle(
                Username.Create(username),
                EmailAddress.Create(request.Email),
                PersonName.Create(request.FirstName, "First name"),
                PersonName.CreateOptional(request.MiddleName, "Middle name"),
                PersonName.Create(request.LastName, "Last name"),
                request.GoogleId,
                request.ProfilePictureUrl,
                defaultRole);

            await _userRepository.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var selfConversation = Conversation.CreateSelf(user.Id);
            await _conversationRepository.AddAsync(selfConversation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var tokens = _jwtTokenService.GenerateTokens(user);
        await _refreshTokenRepository.AddAsync(
            Domain.Entities.RefreshToken.Create(user.Id, tokens.RefreshToken, tokens.RefreshTokenExpiresAt),
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(tokens.AccessToken, tokens.AccessTokenExpiresAt, user.ToDto(), tokens.RefreshToken);
    }

    private async Task<string> ResolveUniqueUsernameAsync(string baseUsername, CancellationToken cancellationToken)
    {
        var candidate = baseUsername;
        var suffix = 1;
        while (await _userRepository.UsernameExistsAsync(candidate, cancellationToken))
        {
            candidate = $"{baseUsername}{suffix++}";
        }

        return Username.Create(candidate).Value;
    }
}

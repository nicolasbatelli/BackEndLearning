using FluentValidation;
using MediatR;
using SocialChat.Application.Abstractions;
using SocialChat.Application.Abstractions.Repositories;
using SocialChat.Application.Common;
using SocialChat.Domain.Entities;

namespace SocialChat.Application.Auth.Commands.Login;

public record LoginCommand(string UsernameOrEmail, string Password) : IRequest<AuthResponse>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UsernameOrEmail).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = request.UsernameOrEmail.Contains('@')
            ? await _userRepository.GetByEmailAsync(request.UsernameOrEmail, cancellationToken)
            : await _userRepository.GetByUsernameAsync(request.UsernameOrEmail, cancellationToken);

        if (user is null || string.IsNullOrEmpty(user.PasswordHash) || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new BusinessRuleException("Invalid credentials.");
        }

        if (!user.IsEmailVerified)
        {
            throw new BusinessRuleException("Please verify your email before signing in.");
        }

        var tokens = _jwtTokenService.GenerateTokens(user);
        await _refreshTokenRepository.AddAsync(
            Domain.Entities.RefreshToken.Create(user.Id, tokens.RefreshToken, tokens.RefreshTokenExpiresAt),
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(tokens.AccessToken, tokens.AccessTokenExpiresAt, user.ToDto(), tokens.RefreshToken);
    }
}

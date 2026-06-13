using FluentValidation;
using MediatR;
using SocialChat.Application.Abstractions;
using SocialChat.Application.Abstractions.Repositories;
using SocialChat.Application.Common;
using SocialChat.Domain.Entities;

namespace SocialChat.Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken)
            ?? throw new BusinessRuleException("Invalid refresh token.");

        if (!storedToken.IsActive)
        {
            throw new BusinessRuleException("Refresh token is expired or revoked.");
        }

        var user = await _userRepository.GetByIdAsync(storedToken.UserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        storedToken.Revoke();
        var tokens = _jwtTokenService.GenerateTokens(user);
        await _refreshTokenRepository.AddAsync(
            Domain.Entities.RefreshToken.Create(user.Id, tokens.RefreshToken, tokens.RefreshTokenExpiresAt),
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(tokens.AccessToken, tokens.AccessTokenExpiresAt, user.ToDto(), tokens.RefreshToken);
    }
}

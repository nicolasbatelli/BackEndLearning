using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using SocialChat.Application.Abstractions;
using SocialChat.Application.Abstractions.Repositories;
using SocialChat.Application.Common;
using SocialChat.Domain.Entities;
using SocialChat.Domain.ValueObjects;

namespace SocialChat.Application.Auth.Commands.RegisterUser;

public record RegisterUserCommand(
    string Username,
    string Password,
    string FirstName,
    string? MiddleName,
    string LastName,
    string Email) : IRequest<RegisterUserResult>;

public record RegisterUserResult(Guid UserId, string Message);

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .Must(BeValidUsername).WithMessage("Username may contain only letters, numbers, and underscores and be 3-30 characters.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .Must(BeStrongPassword).WithMessage("Password must be at least 8 characters with uppercase, lowercase, digit, and special character.");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .Must(BeLettersOnly).WithMessage("First name may contain only letters.");

        RuleFor(x => x.MiddleName)
            .Must(x => string.IsNullOrWhiteSpace(x) || BeLettersOnly(x))
            .WithMessage("Middle name may contain only letters.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .Must(BeLettersOnly).WithMessage("Last name may contain only letters.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }

    private static bool BeValidUsername(string username)
    {
        try
        {
            Username.Create(username);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool BeStrongPassword(string password)
    {
        try
        {
            Password.Create(password);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool BeLettersOnly(string name)
    {
        try
        {
            PersonName.Create(name, "Name");
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailSender _emailSender;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IEmailSender emailSender,
        IUnitOfWork unitOfWork,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _emailSender = emailSender;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.UsernameExistsAsync(request.Username, cancellationToken))
        {
            throw new BusinessRuleException("Username is already taken.");
        }

        if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new BusinessRuleException("Email is already registered.");
        }

        var defaultRole = await _roleRepository.GetByNameAsync(Role.UserRole, cancellationToken)
            ?? throw new BusinessRuleException("Default role is not configured.");

        var username = Username.Create(request.Username);
        var email = EmailAddress.Create(request.Email);
        var firstName = PersonName.Create(request.FirstName, "First name");
        var middleName = PersonName.CreateOptional(request.MiddleName, "Middle name");
        var lastName = PersonName.Create(request.LastName, "Last name");
        Password.Create(request.Password);

        var verificationToken = Guid.NewGuid().ToString("N");
        var user = User.Register(
            username,
            email,
            firstName,
            middleName,
            lastName,
            _passwordHasher.Hash(request.Password),
            verificationToken,
            DateTime.UtcNow.AddHours(24),
            defaultRole);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var verificationLink = $"http://localhost:3000/verify-email?token={verificationToken}&email={Uri.EscapeDataString(email.Value)}";

        try
        {
            await _emailSender.SendEmailVerificationAsync(email.Value, username.Value, verificationLink, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to send verification email to {Email}. Use this link to verify manually: {VerificationLink}",
                email.Value,
                verificationLink);
        }

        return new RegisterUserResult(user.Id, "Registration successful. Please verify your email.");
    }
}

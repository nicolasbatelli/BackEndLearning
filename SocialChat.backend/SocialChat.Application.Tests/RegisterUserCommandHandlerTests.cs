using Moq;
using NUnit.Framework;
using SocialChat.Application.Abstractions;
using SocialChat.Application.Abstractions.Repositories;
using SocialChat.Application.Auth.Commands.RegisterUser;
using SocialChat.Domain.Entities;

namespace SocialChat.Application.Tests;

public class RegisterUserCommandHandlerTests
{
    [Test]
    public async Task Handle_WithExistingUsername_ThrowsBusinessRuleException()
    {
        // Arrange
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.UsernameExistsAsync("taken_user", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new RegisterUserCommandHandler(
            userRepository.Object,
            Mock.Of<IRoleRepository>(),
            Mock.Of<IPasswordHasher>(),
            Mock.Of<IEmailSender>(),
            Mock.Of<IUnitOfWork>());

        var command = new RegisterUserCommand(
            "taken_user",
            "StrongPass1!",
            "John",
            null,
            "Doe",
            "john@example.com");

        // Act & Assert
        Assert.ThrowsAsync<Common.BusinessRuleException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_WithValidData_RegistersUserAndSendsEmail()
    {
        // Arrange
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.UsernameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        userRepository.Setup(x => x.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var roleRepository = new Mock<IRoleRepository>();
        roleRepository.Setup(x => x.GetByNameAsync(Role.UserRole, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Role.Create(Role.UserRole));

        var emailSender = new Mock<IEmailSender>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashed");

        var handler = new RegisterUserCommandHandler(
            userRepository.Object,
            roleRepository.Object,
            passwordHasher.Object,
            emailSender.Object,
            unitOfWork.Object);

        var command = new RegisterUserCommand(
            "new_user",
            "StrongPass1!",
            "John",
            null,
            "Doe",
            "john@example.com");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.UserId, Is.Not.EqualTo(Guid.Empty));
        userRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        emailSender.Verify(
            x => x.SendEmailVerificationAsync("john@example.com", "new_user", It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

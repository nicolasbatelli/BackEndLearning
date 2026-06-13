using MediatR;
using SocialChat.Application.Abstractions;
using SocialChat.Application.Abstractions.Repositories;
using SocialChat.Application.Common;

namespace SocialChat.Application.Auth.Commands.UploadAvatar;

public record UploadAvatarCommand(Guid UserId, Stream FileStream, string ContentType, string FileName) : IRequest<UserDto>;

public class UploadAvatarCommandHandler : IRequestHandler<UploadAvatarCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IImageProcessor _imageProcessor;
    private readonly IUnitOfWork _unitOfWork;

    public UploadAvatarCommandHandler(
        IUserRepository userRepository,
        IImageProcessor imageProcessor,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _imageProcessor = imageProcessor;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var thumbnail = await _imageProcessor.CreateThumbnailAsync(request.FileStream, cancellationToken);

        user.SetProfilePicture(thumbnail.Data, thumbnail.ContentType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return user.ToDto();
    }
}

namespace SocialChat.Application.Abstractions;

public record ProfileThumbnail(byte[] Data, string ContentType);

public interface IImageProcessor
{
    Task<ProfileThumbnail> CreateThumbnailAsync(Stream imageStream, CancellationToken cancellationToken = default);
}

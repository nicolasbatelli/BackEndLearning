using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SocialChat.Application.Abstractions;

namespace SocialChat.Infrastructure.Services;

public class ImageSharpImageProcessor : IImageProcessor
{
    private const int ThumbnailSize = 128;
    private const string OutputContentType = "image/jpeg";

    public async Task<ProfileThumbnail> CreateThumbnailAsync(Stream imageStream, CancellationToken cancellationToken = default)
    {
        using var image = await Image.LoadAsync(imageStream, cancellationToken);

        image.Mutate(context => context.Resize(new ResizeOptions
        {
            Size = new Size(ThumbnailSize, ThumbnailSize),
            Mode = ResizeMode.Crop
        }));

        using var output = new MemoryStream();
        await image.SaveAsJpegAsync(output, new JpegEncoder { Quality = 80 }, cancellationToken);
        return new ProfileThumbnail(output.ToArray(), OutputContentType);
    }
}

using SocialChat.Application.Common;
using SocialChat.Domain.Entities;

namespace SocialChat.Application.Common;

public static class MappingExtensions
{
    public static UserDto ToDto(this User user) =>
        new(
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.MiddleName,
            user.LastName,
            user.FullName,
            user.ResolveProfilePicture(),
            user.IsEmailVerified,
            user.Roles.Select(r => r.Name).ToList());

    public static string? ResolveProfilePicture(this User user)
    {
        if (user.ProfilePictureThumbnail is { Length: > 0 })
        {
            var contentType = string.IsNullOrWhiteSpace(user.ProfilePictureContentType)
                ? "image/jpeg"
                : user.ProfilePictureContentType;
            return $"data:{contentType};base64,{Convert.ToBase64String(user.ProfilePictureThumbnail)}";
        }

        return user.ProfilePictureUrl;
    }
}

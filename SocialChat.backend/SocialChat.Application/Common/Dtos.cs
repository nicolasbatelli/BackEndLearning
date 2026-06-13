using System.Text.Json.Serialization;

namespace SocialChat.Application.Common;

public record UserDto(
    Guid Id,
    string Username,
    string Email,
    string FirstName,
    string? MiddleName,
    string LastName,
    string FullName,
    string? ProfilePictureUrl,
    bool IsEmailVerified,
    IReadOnlyList<string> Roles);

public record AuthResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    UserDto User,
    [property: JsonIgnore] string RefreshToken);

public record ConversationDto(
    Guid Id,
    string Type,
    string? Name,
    string? LastMessage,
    DateTime? LastMessageAt,
    bool IsFavorite,
    IReadOnlyList<ConversationParticipantDto> Participants);

public record ConversationParticipantDto(
    Guid UserId,
    string Username,
    string FullName,
    string? ProfilePictureUrl);

public record MessageDto(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    string SenderUsername,
    string Content,
    DateTime SentAt);

public record NotificationDto(
    Guid Id,
    string Type,
    string Title,
    string Message,
    bool IsRead,
    Guid? RelatedEntityId,
    DateTime CreatedAt);

public record FriendshipDto(
    Guid Id,
    Guid RequesterId,
    string RequesterUsername,
    Guid AddresseeId,
    string AddresseeUsername,
    string Status,
    DateTime CreatedAt);

public record UserSearchResultDto(
    Guid Id,
    string Username,
    string FullName,
    string? ProfilePictureUrl,
    string FriendshipStatus);

public record FriendDto(
    Guid Id,
    string Username,
    string FullName,
    string? ProfilePictureUrl);

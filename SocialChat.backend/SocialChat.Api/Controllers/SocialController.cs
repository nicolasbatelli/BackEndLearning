using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialChat.Application.Common;
using SocialChat.Application.Social.Commands.MarkNotificationRead;
using SocialChat.Application.Social.Commands.RespondFriendInvite;
using SocialChat.Application.Social.Commands.SendFriendInvite;
using SocialChat.Application.Social.Commands.ToggleFavorite;
using SocialChat.Application.Social.Queries.GetFriends;
using SocialChat.Application.Social.Queries.GetNotifications;
using SocialChat.Application.Social.Queries.SearchUsers;

namespace SocialChat.Api.Controllers;

[Authorize(Policy = "RequireAuthenticatedUser")]
[ApiController]
[Route("api/[controller]")]
public class SocialController : ControllerBase
{
    private readonly IMediator _mediator;

    public SocialController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("users/search")]
    public async Task<ActionResult<IReadOnlyList<UserSearchResultDto>>> SearchUsers([FromQuery] string query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SearchUsersQuery(GetUserId(), query), cancellationToken);
        return Ok(result);
    }

    [HttpPost("friends/invite")]
    public async Task<ActionResult<FriendshipDto>> SendInvite([FromBody] SendInviteRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SendFriendInviteCommand(GetUserId(), request.AddresseeId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("friends/respond")]
    public async Task<ActionResult<FriendshipDto>> RespondInvite([FromBody] RespondInviteRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RespondFriendInviteCommand(GetUserId(), request.FriendshipId, request.Accept),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("friends")]
    public async Task<ActionResult<IReadOnlyList<FriendDto>>> GetFriends(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFriendsQuery(GetUserId()), cancellationToken);
        return Ok(result);
    }

    [HttpGet("notifications")]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetNotifications(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetNotificationsQuery(GetUserId()), cancellationToken);
        return Ok(result);
    }

    [HttpPost("notifications/{notificationId:guid}/read")]
    public async Task<IActionResult> MarkNotificationRead(Guid notificationId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarkNotificationReadCommand(GetUserId(), notificationId), cancellationToken);
        return Ok(new { message = "Notification marked as read." });
    }

    [HttpPost("favorites")]
    public async Task<IActionResult> ToggleFavorite([FromBody] ToggleFavoriteRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ToggleFavoriteCommand(GetUserId(), request.ConversationId, request.IsFavorite), cancellationToken);
        return Ok(new { message = "Favorite updated." });
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
}

public record SendInviteRequest(Guid AddresseeId);
public record RespondInviteRequest(Guid FriendshipId, bool Accept);
public record ToggleFavoriteRequest(Guid ConversationId, bool IsFavorite);

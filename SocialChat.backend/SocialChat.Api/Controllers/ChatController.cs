using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialChat.Application.Chat.Commands.CreateConversation;
using SocialChat.Application.Chat.Commands.SendMessage;
using SocialChat.Application.Chat.Queries.GetConversations;
using SocialChat.Application.Chat.Queries.GetMessages;
using SocialChat.Application.Common;

namespace SocialChat.Api.Controllers;

[Authorize(Policy = "RequireAuthenticatedUser")]
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("conversations")]
    public async Task<ActionResult<IReadOnlyList<ConversationDto>>> GetConversations(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetConversationsQuery(GetUserId()), cancellationToken);
        return Ok(result);
    }

    [HttpGet("conversations/{conversationId:guid}/messages")]
    public async Task<ActionResult<IReadOnlyList<MessageDto>>> GetMessages(Guid conversationId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMessagesQuery(GetUserId(), conversationId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("conversations")]
    public async Task<ActionResult<ConversationDto>> CreateConversation([FromBody] CreateConversationRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateConversationCommand(GetUserId(), request.Type, request.Name, request.ParticipantIds),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("messages")]
    public async Task<ActionResult<MessageDto>> SendMessage([FromBody] SendMessageRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SendMessageCommand(GetUserId(), request.ConversationId, request.Content),
            cancellationToken);
        return Ok(result);
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
}

public record CreateConversationRequest(string Type, string? Name, IReadOnlyList<Guid>? ParticipantIds);
public record SendMessageRequest(Guid ConversationId, string Content);

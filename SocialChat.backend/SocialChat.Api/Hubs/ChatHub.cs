using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SocialChat.Application.Chat.Commands.SendMessage;
using SocialChat.Application.Common;

namespace SocialChat.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMediator _mediator;

    public ChatHub(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task JoinConversation(Guid conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
    }

    public async Task LeaveConversation(Guid conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());
    }

    public async Task SendMessage(Guid conversationId, string content)
    {
        var userId = Context.GetUserId();
        var message = await _mediator.Send(new SendMessageCommand(userId, conversationId, content));
        await Clients.Group(conversationId.ToString()).SendAsync("ReceiveMessage", message);
    }
}

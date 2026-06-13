using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SocialChat.Api.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.GetUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.GetUserId();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.ToString());
        await base.OnDisconnectedAsync(exception);
    }
}

public interface INotificationHubClient
{
    Task ReceiveNotification(object notification);
}

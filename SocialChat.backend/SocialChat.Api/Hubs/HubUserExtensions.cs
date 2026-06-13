using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace SocialChat.Api.Hubs;

public static class HubUserExtensions
{
    public static Guid GetUserId(this HubCallerContext context)
    {
        var value = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? context.User?.FindFirst("sub")?.Value;

        if (!Guid.TryParse(value, out var userId))
        {
            throw new HubException("Authenticated user id is missing from the connection.");
        }

        return userId;
    }
}

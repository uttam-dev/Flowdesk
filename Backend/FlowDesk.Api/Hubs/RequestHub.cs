using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace FlowDesk.Api.Hubs;

[Authorize]
public class RequestHub(ILogger<RequestHub> logger) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        var roleIdStr = Context.User?.FindFirst("RoleId")?.Value;

        if (!string.IsNullOrEmpty(userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

        if (!string.IsNullOrEmpty(role))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"role-{role}");

        // Add Support users to "Support" group (RoleId == 4)
        if (int.TryParse(roleIdStr, out int roleId) && roleId == 4)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Support");
        }

        logger.LogInformation(
            "SignalR connected: ConnectionId={ConnectionId} UserId={UserId} Role={Role} RoleId={RoleId}",
            Context.ConnectionId, userId ?? "anonymous", role ?? "none", roleIdStr ?? "none");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (exception is null)
            logger.LogInformation(
                "SignalR disconnected gracefully: ConnectionId={ConnectionId} UserId={UserId}",
                Context.ConnectionId, userId ?? "anonymous");
        else
            logger.LogWarning(exception,
                "SignalR disconnected with error: ConnectionId={ConnectionId} UserId={UserId}",
                Context.ConnectionId, userId ?? "anonymous");

        await base.OnDisconnectedAsync(exception);
    }
}
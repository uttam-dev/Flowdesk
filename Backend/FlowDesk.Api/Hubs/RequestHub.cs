using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace FlowDesk.Api.Hubs;

/// <summary>
/// SignalR hub for real-time request lifecycle notifications.
/// </summary>
/// <remarks>
/// <para><strong>Group naming convention</strong> (must match <c>SignalRService</c> exactly):</para>
/// <list type="table">
///   <listheader>
///     <term>Group name</term>
///     <description>Members</description>
///   </listheader>
///   <item>
///     <term><c>user-{userId}</c></term>
///     <description>
///       One group per authenticated user. Every connection for that user joins
///       their own group so notifications reach all browser tabs/devices.
///     </description>
///   </item>
///   <item>
///     <term><c>role-Admin</c></term>
///     <description>All currently connected Admin users.</description>
///   </item>
///   <item>
///     <term><c>role-Manager</c></term>
///     <description>All currently connected Manager users.</description>
///   </item>
///   <item>
///     <term><c>role-Support</c></term>
///     <description>All currently connected Support users.</description>
///   </item>
///   <item>
///     <term><c>role-Employee</c></term>
///     <description>All currently connected Employee users.</description>
///   </item>
/// </list>
/// <para>
///   ⚠️ IMPORTANT: Always use <c>"role-{RoleName}"</c> (with the <c>"role-"</c> prefix)
///   in <c>SignalRService</c> when broadcasting to a role group.  Using the bare role name
///   (e.g., <c>"Support"</c> instead of <c>"role-Support"</c>) will silently fail because
///   no connection is ever registered in a group with that name.
/// </para>
/// <para><strong>Client events broadcast by <c>SignalRService</c>:</strong></para>
/// <list type="bullet">
///   <item><c>"RequestUpdated"</c>       — request created (legacy event name preserved)</item>
///   <item><c>"RequestStatusUpdated"</c> — approve / reject / in-progress / resolved / escalated</item>
///   <item><c>"RequestAssigned"</c>      — request assigned to a support user</item>
/// </list>
/// </remarks>
[Authorize]
public class RequestHub(ILogger<RequestHub> logger) : Hub
{
    /// <summary>
    /// Registers the connected user in their personal group (<c>user-{userId}</c>)
    /// and their role group (<c>role-{role}</c>) on every successful connection.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role   = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

        if (!string.IsNullOrEmpty(userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

        if (!string.IsNullOrEmpty(role))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"role-{role}");

        logger.LogInformation(
            "SignalR connected: ConnectionId={ConnectionId} UserId={UserId} Role={Role}",
            Context.ConnectionId, userId ?? "anonymous", role ?? "none");

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Logs the disconnection reason. SignalR automatically removes the connection
    /// from all groups on disconnect — no manual cleanup needed.
    /// </summary>
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
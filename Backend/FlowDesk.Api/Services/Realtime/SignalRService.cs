using FlowDesk.Api.Hubs;
using FlowDesk.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Api.Services.Realtime;

/// <summary>
/// SignalR implementation of <see cref="IRealtimeService"/>.
/// <para>
///   Group naming convention used by this service (matches <see cref="RequestHub"/>):
///   <list type="bullet">
///     <item><c>user-{userId}</c>  — individual user notification group</item>
///     <item><c>role-Admin</c>     — all connected Admin users</item>
///     <item><c>role-Manager</c>   — all connected Manager users</item>
///     <item><c>role-Support</c>   — all connected Support users</item>
///   </list>
///   All broadcast methods are fire-and-forget from the caller's perspective
///   (<c>Task.WhenAll</c> is awaited internally, but failures are NOT propagated
///   to the command handler — real-time is best-effort by design).
/// </para>
/// </summary>
public class SignalRService(
    IHubContext<RequestHub> _hub,
    ILogger<SignalRService> _logger) : IRealtimeService
{
    // ── Shared payload factory ─────────────────────────────────────────────────

    /// <summary>
    /// Builds the canonical notification payload sent over the wire to all clients.
    /// Using a strongly-typed record ensures the JSON shape is consistent across
    /// all notification methods and is easy to test.
    /// </summary>
    private static object BuildPayload(
        int requestId,
        string action,
        int? triggeredById = null) =>
        new
        {
            RequestId     = requestId,
            Action        = action,
            TriggeredById = triggeredById,
            Timestamp     = DateTime.UtcNow
        };

    // ── Helper: send to a deduped set of user groups + optional role groups ────

    /// <summary>
    /// Sends <paramref name="eventName"/> with <paramref name="payload"/> to every
    /// non-null user ID in <paramref name="userIds"/> (deduped) and every entry
    /// in <paramref name="roleGroups"/>.  All sends run concurrently via
    /// <c>Task.WhenAll</c>.
    /// </summary>
    private async Task BroadcastAsync(
        string eventName,
        object payload,
        IEnumerable<int?> userIds,
        params string[] roleGroups)
    {
        var tasks = new List<Task>();

        // Deduplicate so a user who is both the employee AND the manager
        // (edge-case) does not receive two identical pushes.
        foreach (var uid in userIds.Where(id => id.HasValue).Select(id => id!.Value).Distinct())
            tasks.Add(_hub.Clients.Group($"user-{uid}").SendAsync(eventName, payload));

        foreach (var roleGroup in roleGroups)
            tasks.Add(_hub.Clients.Group(roleGroup).SendAsync(eventName, payload));

        await Task.WhenAll(tasks);
    }

    // ── Existing methods (preserved verbatim — no signature changes) ───────────

    /// <inheritdoc/>
    public async Task NotifyRequestCreatedAsync(int requestId, int employeeId, int? managerId)
    {
        var payload = BuildPayload(requestId, "CREATED");

        _logger.LogInformation(
            "Broadcasting RequestCreated: RequestId={RequestId} EmployeeId={EmployeeId} ManagerId={ManagerId}",
            requestId, employeeId, managerId);

        var tasks = new List<Task>
        {
            _hub.Clients.Group($"user-{employeeId}").SendAsync("RequestUpdated", payload),
            _hub.Clients.Group("role-Admin").SendAsync("RequestUpdated", payload)
        };

        if (managerId.HasValue)
        {
            tasks.Add(_hub.Clients.Group("role-Manager").SendAsync("RequestUpdated", payload));
            tasks.Add(_hub.Clients.Group($"user-{managerId.Value}").SendAsync("RequestUpdated", payload));
        }

        await Task.WhenAll(tasks);

        _logger.LogInformation("RequestCreated broadcast complete for RequestId={RequestId}", requestId);
    }

    /// <inheritdoc/>
    public async Task NotifyRequestUpdatedAsync(int requestId, string action, IEnumerable<int> userIds)
    {
        var payload = BuildPayload(requestId, action);

        _logger.LogInformation(
            "Broadcasting RequestUpdated (legacy): RequestId={RequestId} Action={Action}",
            requestId, action);

        var tasks = userIds.Select(userId =>
            _hub.Clients.Group($"user-{userId}").SendAsync("RequestUpdated", payload));

        await Task.WhenAll(tasks);
    }

    /// <inheritdoc/>
    public async Task NotifyRequestStatusUpdatedAsync(
        int requestId,
        string newStatus,
        int updatedById,
        int employeeId,
        int? assignedToId,
        int? managerId)
    {
        var payload = new
        {
            RequestId     = requestId,
            NewStatus     = newStatus,
            UpdatedById   = updatedById,
            UpdatedAt     = DateTime.UtcNow
        };

        _logger.LogInformation(
            "Broadcasting RequestStatusUpdated: RequestId={RequestId} NewStatus={NewStatus}",
            requestId, newStatus);

        var tasks = new List<Task>
        {
            // Admin always receives status updates
            _hub.Clients.Group("role-Admin").SendAsync("RequestStatusUpdated", payload),

            // Request creator always receives status updates on their own requests
            _hub.Clients.Group($"user-{employeeId}").SendAsync("RequestStatusUpdated", payload)
        };

        // Assigned support user receives updates on their work
        if (assignedToId.HasValue)
            tasks.Add(_hub.Clients.Group($"user-{assignedToId.Value}")
                .SendAsync("RequestStatusUpdated", payload));

        // Employee's manager is kept informed
        if (managerId.HasValue)
            tasks.Add(_hub.Clients.Group($"user-{managerId.Value}")
                .SendAsync("RequestStatusUpdated", payload));

        await Task.WhenAll(tasks);

        _logger.LogInformation(
            "RequestStatusUpdated broadcast complete for RequestId={RequestId}", requestId);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// BUG FIX: The group is <c>"role-Support"</c> (matching <see cref="RequestHub"/>
    /// group registration) — NOT the bare string <c>"Support"</c>.
    /// </remarks>
    public async Task NotifyRequestAssignedAsync(
        int requestId,
        int assignedToId,
        string assignedToName,
        int employeeId,
        int? managerId)
    {
        var payload = new
        {
            RequestId      = requestId,
            AssignedToId   = assignedToId,
            AssignedToName = assignedToName,
            AssignedAt     = DateTime.UtcNow
        };

        _logger.LogInformation(
            "Broadcasting RequestAssigned: RequestId={RequestId} AssignedToId={AssignedToId}",
            requestId, assignedToId);

        var tasks = new List<Task>
        {
            // All online support agents see the new assignment in their queue (FIXED: was "Support")
            _hub.Clients.Group("role-Support").SendAsync("RequestAssigned", payload),

            // The specific support user who was assigned
            _hub.Clients.Group($"user-{assignedToId}").SendAsync("RequestAssigned", payload),

            // Admin always receives assignment events
            _hub.Clients.Group("role-Admin").SendAsync("RequestAssigned", payload),

            // Employee is notified their request has been picked up
            _hub.Clients.Group($"user-{employeeId}").SendAsync("RequestAssigned", payload)
        };

        // Manager is informed of the assignment
        if (managerId.HasValue)
            tasks.Add(_hub.Clients.Group($"user-{managerId.Value}")
                .SendAsync("RequestAssigned", payload));

        await Task.WhenAll(tasks);

        _logger.LogInformation(
            "RequestAssigned broadcast complete for RequestId={RequestId}", requestId);
    }

    // ── Typed semantic methods — full request lifecycle ────────────────────────

    /// <inheritdoc/>
    public async Task NotifyRequestApprovedAsync(
        int requestId,
        int employeeId,
        int? managerId,
        int approvedById)
    {
        _logger.LogInformation(
            "Broadcasting RequestApproved: RequestId={RequestId} ApprovedById={ApprovedById}",
            requestId, approvedById);

        var payload = BuildPayload(requestId, "Approved", approvedById);

        await BroadcastAsync(
            "RequestStatusUpdated",
            payload,
            userIds:    [employeeId, managerId],
            roleGroups: "role-Admin");

        _logger.LogInformation(
            "RequestApproved broadcast complete for RequestId={RequestId}", requestId);
    }

    /// <inheritdoc/>
    public async Task NotifyRequestRejectedAsync(
        int requestId,
        int employeeId,
        int? managerId,
        int rejectedById)
    {
        _logger.LogInformation(
            "Broadcasting RequestRejected: RequestId={RequestId} RejectedById={RejectedById}",
            requestId, rejectedById);

        var payload = BuildPayload(requestId, "Rejected", rejectedById);

        await BroadcastAsync(
            "RequestStatusUpdated",
            payload,
            userIds:    [employeeId, managerId],
            roleGroups: "role-Admin");

        _logger.LogInformation(
            "RequestRejected broadcast complete for RequestId={RequestId}", requestId);
    }

    /// <inheritdoc/>
    public async Task NotifyRequestStartedAsync(
        int requestId,
        int employeeId,
        int? managerId,
        int supportId)
    {
        _logger.LogInformation(
            "Broadcasting RequestStarted (InProgress): RequestId={RequestId} SupportId={SupportId}",
            requestId, supportId);

        var payload = BuildPayload(requestId, "InProgress", supportId);

        await BroadcastAsync(
            "RequestStatusUpdated",
            payload,
            userIds:    [employeeId, managerId, supportId],
            roleGroups: "role-Admin");

        _logger.LogInformation(
            "RequestStarted broadcast complete for RequestId={RequestId}", requestId);
    }

    /// <inheritdoc/>
    public async Task NotifyRequestResolvedAsync(
        int requestId,
        int employeeId,
        int? managerId,
        int supportId)
    {
        _logger.LogInformation(
            "Broadcasting RequestResolved: RequestId={RequestId} SupportId={SupportId}",
            requestId, supportId);

        var payload = BuildPayload(requestId, "Resolved", supportId);

        await BroadcastAsync(
            "RequestStatusUpdated",
            payload,
            userIds:    [employeeId, managerId, supportId],
            roleGroups: "role-Admin");

        _logger.LogInformation(
            "RequestResolved broadcast complete for RequestId={RequestId}", requestId);
    }

    /// <inheritdoc/>
    public async Task NotifyRequestEscalatedAsync(
        int requestId,
        int employeeId,
        int? managerId,
        int? assignedToId,
        int escalatedById)
    {
        _logger.LogInformation(
            "Broadcasting RequestEscalated: RequestId={RequestId} EscalatedById={EscalatedById} " +
            "AssignedToId={AssignedToId}",
            requestId, escalatedById, assignedToId);

        var payload = BuildPayload(requestId, "Escalated", escalatedById);

        await BroadcastAsync(
            "RequestStatusUpdated",
            payload,
            userIds:    [employeeId, managerId, assignedToId],
            roleGroups: "role-Admin");

        _logger.LogInformation(
            "RequestEscalated broadcast complete for RequestId={RequestId}", requestId);
    }
}
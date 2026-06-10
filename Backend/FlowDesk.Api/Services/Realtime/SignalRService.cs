using FlowDesk.Api.Hubs;
using FlowDesk.Application.Common.Interfaces;
using FlowDesk.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Api.Services.Realtime;

public class SignalRService(
    IHubContext<RequestHub> _hub,
    ILogger<SignalRService> _logger,
    IRequestRepository _repository) : IRealtimeService
{

    private static object BuildPayload(
        int requestId,
        string requestNumber,
        string action,
        int? triggeredById = null) =>
        new
        {
            RequestId = requestId,
            RequestNumber = requestNumber,
            Action = action,
            TriggeredById = triggeredById,
            Timestamp = DateTime.UtcNow
        };


    private async Task<string> GetRequestNumberAsync(int requestId)
    {
        var number = await _repository.GetRequestNumberByIdAsync(requestId);
        return number ?? string.Empty;
    }

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
        var requestNumber = await GetRequestNumberAsync(requestId);
        var payload = BuildPayload(requestId, requestNumber, "CREATED");

        _logger.LogInformation(
            "Broadcasting RequestCreated: RequestId={RequestId} RequestNumber={RequestNumber} EmployeeId={EmployeeId} ManagerId={ManagerId}",
            requestId, requestNumber, employeeId, managerId);

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
        var requestNumber = await GetRequestNumberAsync(requestId);
        var payload = BuildPayload(requestId, requestNumber, action);

        _logger.LogInformation(
            "Broadcasting RequestUpdated (legacy): RequestId={RequestId} RequestNumber={RequestNumber} Action={Action}",
            requestId, requestNumber, action);

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
        var requestNumber = await GetRequestNumberAsync(requestId);
        var payload = new
        {
            RequestId = requestId,
            RequestNumber = requestNumber,
            NewStatus = newStatus,
            UpdatedById = updatedById,
            UpdatedAt = DateTime.UtcNow
        };

        _logger.LogInformation(
            "Broadcasting RequestStatusUpdated: RequestId={RequestId} RequestNumber={RequestNumber} NewStatus={NewStatus}",
            requestId, requestNumber, newStatus);

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


    public async Task NotifyRequestAssignedAsync(
        int requestId,
        int assignedToId,
        string assignedToName,
        int employeeId,
        int? managerId)
    {
        var requestNumber = await GetRequestNumberAsync(requestId);
        var payload = new
        {
            RequestId = requestId,
            RequestNumber = requestNumber,
            AssignedToId = assignedToId,
            AssignedToName = assignedToName,
            AssignedAt = DateTime.UtcNow
        };

        _logger.LogInformation(
            "Broadcasting RequestAssigned: RequestId={RequestId} RequestNumber={RequestNumber} AssignedToId={AssignedToId}",
            requestId, requestNumber, assignedToId);

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
        var requestNumber = await GetRequestNumberAsync(requestId);
        var payload = BuildPayload(requestId, requestNumber, "Approved", approvedById);

        _logger.LogInformation(
            "Broadcasting RequestApproved: RequestId={RequestId} RequestNumber={RequestNumber} ApprovedById={ApprovedById}",
            requestId, requestNumber, approvedById);

        await BroadcastAsync(
            "RequestStatusUpdated",
            payload,
            userIds: [employeeId, managerId],
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
        var requestNumber = await GetRequestNumberAsync(requestId);
        var payload = BuildPayload(requestId, requestNumber, "Rejected", rejectedById);

        _logger.LogInformation(
            "Broadcasting RequestRejected: RequestId={RequestId} RequestNumber={RequestNumber} RejectedById={RejectedById}",
            requestId, requestNumber, rejectedById);

        await BroadcastAsync(
            "RequestStatusUpdated",
            payload,
            userIds: [employeeId, managerId],
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
        var requestNumber = await GetRequestNumberAsync(requestId);
        var payload = BuildPayload(requestId, requestNumber, "InProgress", supportId);

        _logger.LogInformation(
            "Broadcasting RequestStarted (InProgress): RequestId={RequestId} RequestNumber={RequestNumber} SupportId={SupportId}",
            requestId, requestNumber, supportId);

        await BroadcastAsync(
            "RequestStatusUpdated",
            payload,
            userIds: [employeeId, managerId, supportId],
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
        var requestNumber = await GetRequestNumberAsync(requestId);
        var payload = BuildPayload(requestId, requestNumber, "Resolved", supportId);

        _logger.LogInformation(
            "Broadcasting RequestResolved: RequestId={RequestId} RequestNumber={RequestNumber} SupportId={SupportId}",
            requestId, requestNumber, supportId);

        await BroadcastAsync(
            "RequestStatusUpdated",
            payload,
            userIds: [employeeId, managerId, supportId],
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
        var requestNumber = await GetRequestNumberAsync(requestId);
        var payload = BuildPayload(requestId, requestNumber, "Escalated", escalatedById);

        _logger.LogInformation(
            "Broadcasting RequestEscalated: RequestId={RequestId} RequestNumber={RequestNumber} EscalatedById={EscalatedById} " +
            "AssignedToId={AssignedToId}",
            requestId, requestNumber, escalatedById, assignedToId);

        await BroadcastAsync(
            "RequestStatusUpdated",
            payload,
            userIds: [employeeId, managerId, assignedToId],
            roleGroups: "role-Admin");

        _logger.LogInformation(
            "RequestEscalated broadcast complete for RequestId={RequestId}", requestId);
    }

    public async Task NotifyRemoteSessionInitiatedAsync(
    int targetUserId,
    int sessionId,
    int requestId,
    string supportName,
    CancellationToken ct = default)
    {
        await _hub.Clients
            .Group(targetUserId.ToString())
            .SendAsync("RemoteSessionInitiated", new
            {
                sessionId,
                requestId,
                supportName,
                message = $"{supportName} is requesting remote access to your machine."
            }, ct);
    }

    public async Task NotifyRemoteSessionAcceptedAsync(
        int supportUserId,
        int sessionId,
        CancellationToken ct = default)
    {
        await _hub.Clients
            .Group(supportUserId.ToString())
            .SendAsync("RemoteSessionAccepted", new { sessionId }, ct);
    }

    public async Task NotifyRemoteSessionRejectedAsync(
        int supportUserId,
        int sessionId,
        string? rejectionReason,
        CancellationToken ct = default)
    {
        await _hub.Clients
            .Group(supportUserId.ToString())
            .SendAsync("RemoteSessionRejected", new { sessionId, rejectionReason }, ct);
    }

    public async Task NotifyRemoteSessionEndedAsync(
        int targetUserId,
        int supportUserId,
        int sessionId,
        bool resolved,
        CancellationToken ct = default)
    {
        await _hub.Clients
            .Groups(targetUserId.ToString(), supportUserId.ToString())
            .SendAsync("RemoteSessionEnded", new { sessionId, resolved }, ct);
    }
}

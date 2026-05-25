using FlowDesk.Api.Hubs;
using FlowDesk.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Api.Services.Realtime;

public class SignalRService(
    IHubContext<RequestHub> _hub,
    ILogger<SignalRService> _logger) : IRealtimeService
{
    // ── Existing method ───────────────────────────────────────────────────────

    /// <summary>
    /// Notifies relevant role-groups and the creating employee when a new request
    /// is created.
    /// <para>
    ///   Employee creates → notifies: role-Manager, role-Admin, user-{employeeId},
    ///   user-{managerId}.
    ///   Manager creates → notifies: role-Admin, user-{employeeId}.
    /// </para>
    /// </summary>
    public async Task NotifyRequestCreatedAsync(int requestId, int employeeId, int? managerId)
    {
        var payload = new
        {
            RequestId = requestId,
            Action = "CREATED",
            CreatedAt = DateTime.UtcNow
        };

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

    /// <summary>
    /// Notifies a specific set of users (by userId) of a generic update.
    /// Legacy method kept for compatibility.
    /// </summary>
    public async Task NotifyRequestUpdatedAsync(int requestId, string action, IEnumerable<int> userIds)
    {
        var payload = new
        {
            RequestId = requestId,
            Action = action,
            CreatedAt = DateTime.UtcNow
        };

        _logger.LogInformation(
            "Broadcasting RequestUpdated (legacy): RequestId={RequestId} Action={Action}",
            requestId, action);

        var tasks = userIds.Select(userId =>
            _hub.Clients.Group($"user-{userId}").SendAsync("RequestUpdated", payload));

        await Task.WhenAll(tasks);
    }

    // ── New methods ───────────────────────────────────────────────────────────

    /// <summary>
    /// Sends a <c>RequestStatusUpdated</c> SignalR event to all parties interested
    /// in this request's status change.
    /// </summary>
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
            RequestId = requestId,
            NewStatus = newStatus,
            UpdatedById = updatedById,
            UpdatedAt = DateTime.UtcNow
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
        {
            tasks.Add(_hub.Clients.Group($"user-{assignedToId.Value}")
                .SendAsync("RequestStatusUpdated", payload));
        }

        // Employee's manager is kept informed
        if (managerId.HasValue)
        {
            tasks.Add(_hub.Clients.Group($"user-{managerId.Value}")
                .SendAsync("RequestStatusUpdated", payload));
        }

        await Task.WhenAll(tasks);

        _logger.LogInformation(
            "RequestStatusUpdated broadcast complete for RequestId={RequestId}", requestId);
    }

    /// <summary>
    /// Sends a <c>RequestAssigned</c> SignalR event to the Support role group,
    /// the specific assignee, and the Admin role group.
    /// </summary>
    public async Task NotifyRequestAssignedAsync(
        int requestId,
        int assignedToId,
        string assignedToName,
        int employeeId,
        int? managerId)
    {
        var payload = new
        {
            RequestId = requestId,
            AssignedToId = assignedToId,
            AssignedToName = assignedToName,
            AssignedAt = DateTime.UtcNow
        };

        _logger.LogInformation(
            "Broadcasting RequestAssigned: RequestId={RequestId} AssignedToId={AssignedToId}",
            requestId, assignedToId);

        var tasks = new List<Task>
        {
            // All online support agents see the new assignment in their queue
            _hub.Clients.Group("Support").SendAsync("RequestAssigned", payload),

            // The specific support user who was assigned (optional improvement)
            _hub.Clients.User(assignedToId.ToString()).SendAsync("RequestAssigned", payload),

            // Admin always receives assignment events
            _hub.Clients.Group("role-Admin").SendAsync("RequestAssigned", payload)
        };

        await Task.WhenAll(tasks);

        _logger.LogInformation(
            "RequestAssigned broadcast complete for RequestId={RequestId}", requestId);
    }
}
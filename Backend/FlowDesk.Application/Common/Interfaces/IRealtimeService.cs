using System.Collections.Generic;

namespace FlowDesk.Application.Common.Interfaces
{
    /// <summary>
    /// Abstraction for broadcasting real-time notifications to connected clients.
    /// Implementations live in the API layer (SignalRService) so the Application
    /// layer stays free of ASP.NET Core / SignalR dependencies.
    /// </summary>
    public interface IRealtimeService
    {
        // ── Existing methods (do NOT rename or remove — callers depend on them) ──

        /// <summary>
        /// Notifies relevant groups when a new request is created.
        /// Recipients: <c>user-{employeeId}</c>, <c>role-Admin</c>,
        /// and <c>role-Manager</c> / <c>user-{managerId}</c> when managerId is set.
        /// </summary>
        Task NotifyRequestCreatedAsync(int requestId, int employeeId, int? managerId);

        /// <summary>
        /// Notifies a specific set of users (by userId) of a generic request update.
        /// Legacy method — prefer the typed overloads below for new flows.
        /// </summary>
        Task NotifyRequestUpdatedAsync(int requestId, string action, IEnumerable<int> userIds);

        /// <summary>
        /// Sends a <c>RequestStatusUpdated</c> SignalR event to all parties interested
        /// in this request: Admin role group, the employee (creator), the assigned
        /// support user (if any), and the employee's manager (if any).
        /// </summary>
        Task NotifyRequestStatusUpdatedAsync(
            int requestId,
            string newStatus,
            int updatedById,
            int employeeId,
            int? assignedToId,
            int? managerId);

        /// <summary>
        /// Sends a <c>RequestAssigned</c> SignalR event to the Support role group,
        /// the specific assignee's user group, and the Admin role group.
        /// </summary>
        Task NotifyRequestAssignedAsync(
            int requestId,
            int assignedToId,
            string assignedToName,
            int employeeId,
            int? managerId);

        // ── Typed semantic methods — full request lifecycle ────────────────────
        // Each method name describes the exact event. Each parameter represents
        // a stakeholder whose user-group will receive the notification.
        // Implementations in SignalRService (API layer) — no ASP.NET Core leaks
        // into this interface.

        /// <summary>
        /// Notifies stakeholders when a Manager approves a request.
        /// Recipients: <c>user-{employeeId}</c>, <c>user-{managerId}</c> (if set),
        /// <c>role-Admin</c>.
        /// </summary>
        Task NotifyRequestApprovedAsync(
            int requestId,
            int employeeId,
            int? managerId,
            int approvedById);

        /// <summary>
        /// Notifies stakeholders when a Manager rejects a request.
        /// Recipients: <c>user-{employeeId}</c>, <c>user-{managerId}</c> (if set),
        /// <c>role-Admin</c>.
        /// </summary>
        Task NotifyRequestRejectedAsync(
            int requestId,
            int employeeId,
            int? managerId,
            int rejectedById);

        /// <summary>
        /// Notifies stakeholders when a Support user starts working on a request (InProgress).
        /// Recipients: <c>user-{employeeId}</c>, <c>user-{managerId}</c> (if set),
        /// <c>user-{supportId}</c>, <c>role-Admin</c>.
        /// </summary>
        Task NotifyRequestStartedAsync(
            int requestId,
            int employeeId,
            int? managerId,
            int supportId);

        /// <summary>
        /// Notifies stakeholders when a Support user resolves a request.
        /// Recipients: <c>user-{employeeId}</c>, <c>user-{managerId}</c> (if set),
        /// <c>user-{supportId}</c>, <c>role-Admin</c>.
        /// </summary>
        Task NotifyRequestResolvedAsync(
            int requestId,
            int employeeId,
            int? managerId,
            int supportId);

        /// <summary>
        /// Notifies stakeholders when an Admin escalates a request.
        /// Recipients: <c>user-{employeeId}</c>, <c>user-{managerId}</c> (if set),
        /// <c>user-{assignedToId}</c> (if assigned), <c>role-Admin</c>.
        /// </summary>
        Task NotifyRequestEscalatedAsync(
            int requestId,
            int employeeId,
            int? managerId,
            int? assignedToId,
            int escalatedById);
    }
}

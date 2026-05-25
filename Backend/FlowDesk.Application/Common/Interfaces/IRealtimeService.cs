using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Common.Interfaces
{
    public interface IRealtimeService
    {
        // ── Existing methods (do not rename or remove) ────────────────────────

        /// <summary>Notifies relevant groups when a new request is created.</summary>
        Task NotifyRequestCreatedAsync(int requestId, int employeeId, int? managerId);

        /// <summary>
        /// Notifies a specific set of users (by userId) of a generic request update.
        /// Legacy method — prefer the typed overloads below for new flows.
        /// </summary>
        Task NotifyRequestUpdatedAsync(int requestId, string action, IEnumerable<int> userIds);

        // ── New methods added for status-update and assignment flows ──────────

        /// <summary>
        /// Sends a <c>RequestStatusUpdated</c> SignalR event to all parties interested
        /// in this request: Admin role group, the employee (creator), the assigned support
        /// user (if any), and the employee's manager (if any).
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
    }
}

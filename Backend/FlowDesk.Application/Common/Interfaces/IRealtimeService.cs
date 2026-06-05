using System.Collections.Generic;

namespace FlowDesk.Application.Common.Interfaces
{
    public interface IRealtimeService
    {
        Task NotifyRequestCreatedAsync(int requestId, int employeeId, int? managerId);

        Task NotifyRequestUpdatedAsync(int requestId, string action, IEnumerable<int> userIds);

        Task NotifyRequestStatusUpdatedAsync(
            int requestId,
            string newStatus,
            int updatedById,
            int employeeId,
            int? assignedToId,
            int? managerId);

        Task NotifyRequestAssignedAsync(
            int requestId,
            int assignedToId,
            string assignedToName,
            int employeeId,
            int? managerId);

        Task NotifyRequestApprovedAsync(
            int requestId,
            int employeeId,
            int? managerId,
            int approvedById);

        Task NotifyRequestRejectedAsync(
            int requestId,
            int employeeId,
            int? managerId,
            int rejectedById);

        Task NotifyRequestStartedAsync(
            int requestId,
            int employeeId,
            int? managerId,
            int supportId);
        Task NotifyRequestResolvedAsync(
            int requestId,
            int employeeId,
            int? managerId,
            int supportId);

        Task NotifyRequestEscalatedAsync(
            int requestId,
            int employeeId,
            int? managerId,
            int? assignedToId,
            int escalatedById);

        Task NotifyRemoteSessionInitiatedAsync(
       int targetUserId,
       int sessionId,
       int requestId,
       string supportName,
       CancellationToken ct = default);

        Task NotifyRemoteSessionAcceptedAsync(
            int supportUserId,
            int targetUserId,
            int sessionId,
            CancellationToken ct = default);

        Task NotifyRemoteSessionRejectedAsync(
            int supportUserId,
            int sessionId,
            string? rejectionReason,
            CancellationToken ct = default);

        Task NotifyRemoteSessionEndedAsync(
            int targetUserId,
            int supportUserId,
            int sessionId,
            bool resolved,
            CancellationToken ct = default);
    }
}

using FlowDesk.Application.Common.Interfaces;
using FlowDesk.Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.EventHandlers;

/// <summary>
/// Handles <see cref="RequestStatusUpdatedEvent"/> published by the pipeline behaviors
/// for Approve, Reject, and UpdateRequestStatus commands.
/// <para>
///   Notifies over SignalR (via <see cref="IRealtimeService"/>):
///   <list type="bullet">
///     <item><c>role-Admin</c> — always</item>
///     <item><c>user-{EmployeeId}</c> — the request creator</item>
///     <item><c>user-{AssignedToId}</c> — the assigned support user (if any)</item>
///     <item><c>user-{ManagerId}</c> — the employee's manager (if any)</item>
///   </list>
/// </para>
/// </summary>
public sealed class RequestStatusUpdatedEventHandler(
    IRealtimeService realtimeService,
    ILogger<RequestStatusUpdatedEventHandler> logger)
    : INotificationHandler<RequestStatusUpdatedEvent>
{
    public async Task Handle(RequestStatusUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "RequestStatusUpdatedEvent received: RequestId={RequestId} NewStatus={NewStatus} UpdatedById={UpdatedById}",
            notification.RequestId, notification.NewStatus, notification.UpdatedById);

        await realtimeService.NotifyRequestStatusUpdatedAsync(
            requestId: notification.RequestId,
            newStatus: notification.NewStatus,
            updatedById: notification.UpdatedById,
            employeeId: notification.EmployeeId,
            assignedToId: notification.AssignedToId,
            managerId: notification.ManagerId);

        logger.LogInformation(
            "RequestStatusUpdatedEvent handled for RequestId={RequestId}", notification.RequestId);
    }
}

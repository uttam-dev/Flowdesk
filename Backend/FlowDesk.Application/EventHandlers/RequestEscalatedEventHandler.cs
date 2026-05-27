using FlowDesk.Application.Common.Interfaces;
using FlowDesk.Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.EventHandlers;

/// <summary>
/// Handles <see cref="RequestEscalatedEvent"/> published by
/// <see cref="Behaviors.EscalateRequestNotificationBehavior"/> after escalation succeeds.
/// <para>
///   Notifies over SignalR (via <see cref="IRealtimeService"/>):
///   <list type="bullet">
///     <item><c>user-{EmployeeId}</c>    — request creator is informed of the escalation</item>
///     <item><c>user-{ManagerId}</c>     — employee's manager (if any)</item>
///     <item><c>user-{AssignedToId}</c>  — the assigned support user (if any)</item>
///     <item><c>role-Admin</c>           — always notified</item>
///   </list>
/// </para>
/// <para>
///   NOTE: Event handler failures are isolated — a SignalR broadcast failure will NOT
///   roll back the already-committed escalation in the database.
/// </para>
/// </summary>
public sealed class RequestEscalatedEventHandler(
    IRealtimeService realtimeService,
    ILogger<RequestEscalatedEventHandler> logger)
    : INotificationHandler<RequestEscalatedEvent>
{
    public async Task Handle(RequestEscalatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "RequestEscalatedEvent received: RequestId={RequestId} EmployeeId={EmployeeId} " +
            "ManagerId={ManagerId} AssignedToId={AssignedToId} EscalatedById={EscalatedById}",
            notification.RequestId,
            notification.EmployeeId,
            notification.ManagerId,
            notification.AssignedToId,
            notification.EscalatedById);

        await realtimeService.NotifyRequestEscalatedAsync(
            requestId:    notification.RequestId,
            employeeId:   notification.EmployeeId,
            managerId:    notification.ManagerId,
            assignedToId: notification.AssignedToId,
            escalatedById: notification.EscalatedById);

        logger.LogInformation(
            "RequestEscalatedEvent handled for RequestId={RequestId}", notification.RequestId);
    }
}

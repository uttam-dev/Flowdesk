using FlowDesk.Application.Common.Interfaces;
using FlowDesk.Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.EventHandlers;

/// <summary>
/// Handles <see cref="RequestAssignedEvent"/> published by the pipeline behavior
/// for the <c>AssignRequestCommand</c>.
/// <para>
///   Notifies over SignalR (via <see cref="IRealtimeService"/>):
///   <list type="bullet">
///     <item><c>role-Support</c> — all online support agents see new work instantly</item>
///     <item><c>user-{AssignedToId}</c> — the specific support user who was assigned</item>
///     <item><c>role-Admin</c> — always kept in sync</item>
///   </list>
/// </para>
/// </summary>
public sealed class RequestAssignedEventHandler(
    IRealtimeService realtimeService,
    ILogger<RequestAssignedEventHandler> logger)
    : INotificationHandler<RequestAssignedEvent>
{
    public async Task Handle(RequestAssignedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "RequestAssignedEvent received: RequestId={RequestId} AssignedToId={AssignedToId} AssignedToName={AssignedToName}",
            notification.RequestId, notification.AssignedToId, notification.AssignedToName);

        await realtimeService.NotifyRequestAssignedAsync(
            requestId: notification.RequestId,
            assignedToId: notification.AssignedToId,
            assignedToName: notification.AssignedToName,
            employeeId: notification.EmployeeId,
            managerId: notification.ManagerId);

        logger.LogInformation(
            "RequestAssignedEvent handled for RequestId={RequestId}", notification.RequestId);
    }
}

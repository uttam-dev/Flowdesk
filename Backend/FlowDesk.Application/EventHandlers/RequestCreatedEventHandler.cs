using FlowDesk.Application.Common.Interfaces;
using FlowDesk.Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.EventHandlers;

/// <summary>
/// Handles <see cref="RequestCreatedEvent"/>.
/// <para>
///   NOTE: Request creation is currently notified directly inside
///   <c>CreateRequestCommand</c> via <c>IRealtimeService.NotifyRequestCreatedAsync</c>.
///   This handler is a clean migration path — activate it by publishing
///   <see cref="RequestCreatedEvent"/> from a pipeline behavior on that command
///   and removing the direct call from the handler.
/// </para>
/// </summary>
public sealed class RequestCreatedEventHandler(
    IRealtimeService realtimeService,
    ILogger<RequestCreatedEventHandler> logger)
    : INotificationHandler<RequestCreatedEvent>
{
    public async Task Handle(RequestCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "RequestCreatedEvent received: RequestId={RequestId} EmployeeId={EmployeeId} CreatorRole={CreatorRole}",
            notification.RequestId, notification.EmployeeId, notification.CreatorRole);

        // Delegates to IRealtimeService — no direct SignalR dependency in Application layer.
        await realtimeService.NotifyRequestCreatedAsync(
            requestId: notification.RequestId,
            employeeId: notification.EmployeeId,
            managerId: notification.ManagerId);

        logger.LogInformation(
            "RequestCreatedEvent handled for RequestId={RequestId}", notification.RequestId);
    }
}

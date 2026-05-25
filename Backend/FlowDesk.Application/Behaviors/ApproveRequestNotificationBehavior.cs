using FlowDesk.Application.Events;
using FlowDesk.Application.Features.Requests.Commands;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Behaviors;

/// <summary>
/// Pipeline behavior that publishes <see cref="RequestStatusUpdatedEvent"/>
/// AFTER <see cref="ApproveRequestCommandHandler"/> completes successfully.
/// <para>
///   The existing handler logic is fully preserved — this behavior only
///   fires an event post-execution; it does not inject SignalR concerns
///   into the handler.
/// </para>
/// </summary>
public sealed class ApproveRequestNotificationBehavior(
    IPublisher publisher,
    IRequestsService requestsService,
    ILogger<ApproveRequestNotificationBehavior> logger)
    : IPipelineBehavior<ApproveRequestCommand, Unit>
{
    public async Task<Unit> Handle(
        ApproveRequestCommand request,
        RequestHandlerDelegate<Unit> next,
        CancellationToken cancellationToken)
    {
        // Execute existing handler — no changes to its logic.
        var result = await next();

        try
        {
            var ids = await requestsService.GetRespectiveIds(request.RequestId);
            if (ids is not null)
            {
                await publisher.Publish(new RequestStatusUpdatedEvent(
                    RequestId: ids.RequestId,
                    NewStatus: "Approved",
                    UpdatedById: request.UserId,
                    EmployeeId: ids.EmployeeId,
                    AssignedToId: null,          // Approved before assignment
                    ManagerId: ids.ManagerId),
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            // Event publish failure must never roll back the already-committed operation.
            logger.LogError(ex,
                "Failed to publish RequestStatusUpdatedEvent (Approved) for RequestId={RequestId}",
                request.RequestId);
        }

        return result;
    }
}

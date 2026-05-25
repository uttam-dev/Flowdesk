using FlowDesk.Application.Events;
using FlowDesk.Application.Features.Requests.Commands;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Behaviors;

/// <summary>
/// Pipeline behavior that publishes <see cref="RequestStatusUpdatedEvent"/>
/// AFTER <see cref="RejectRequestCommandHandler"/> completes successfully.
/// </summary>
public sealed class RejectRequestNotificationBehavior(
    IPublisher publisher,
    IRequestsService requestsService,
    ILogger<RejectRequestNotificationBehavior> logger)
    : IPipelineBehavior<RejectRequestCommand, Unit>
{
    public async Task<Unit> Handle(
        RejectRequestCommand request,
        RequestHandlerDelegate<Unit> next,
        CancellationToken cancellationToken)
    {
        var result = await next();

        try
        {
            var ids = await requestsService.GetRespectiveIds(request.RequestId);
            if (ids is not null)
            {
                await publisher.Publish(new RequestStatusUpdatedEvent(
                    RequestId: ids.RequestId,
                    NewStatus: "Rejected",
                    UpdatedById: request.UserId,
                    EmployeeId: ids.EmployeeId,
                    AssignedToId: null,
                    ManagerId: ids.ManagerId),
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to publish RequestStatusUpdatedEvent (Rejected) for RequestId={RequestId}",
                request.RequestId);
        }

        return result;
    }
}

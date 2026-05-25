using FlowDesk.Application.Events;
using FlowDesk.Application.Features.Requests.Commands;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Behaviors;

/// <summary>
/// Pipeline behavior that publishes <see cref="RequestStatusUpdatedEvent"/>
/// AFTER <see cref="UpdateRequestStatusCommandHandler"/> completes successfully.
/// <para>
///   The handler may auto-close a Resolved request to Closed; the notification
///   reports the status the user requested (<c>Dto.Status</c>). The frontend
///   will refetch to display the final persisted state.
/// </para>
/// </summary>
public sealed class UpdateRequestStatusNotificationBehavior(
    IPublisher publisher,
    IRequestsService requestsService,
    ILogger<UpdateRequestStatusNotificationBehavior> logger)
    : IPipelineBehavior<UpdateRequestStatusCommand, Unit>
{
    public async Task<Unit> Handle(
        UpdateRequestStatusCommand request,
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
                    NewStatus: request.Dto.Status.ToString(),
                    UpdatedById: request.UserId,
                    EmployeeId: ids.EmployeeId,
                    // Support user updating status IS the assigned user
                    AssignedToId: request.UserId,
                    ManagerId: ids.ManagerId),
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to publish RequestStatusUpdatedEvent (UpdateStatus) for RequestId={RequestId}",
                request.RequestId);
        }

        return result;
    }
}

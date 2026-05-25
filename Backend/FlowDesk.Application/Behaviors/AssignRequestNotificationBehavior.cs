using FlowDesk.Application.Events;
using FlowDesk.Application.Features.Requests.Commands;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Behaviors;

/// <summary>
/// Pipeline behavior that publishes <see cref="RequestAssignedEvent"/>
/// AFTER <see cref="AssignRequestCommandHandler"/> completes successfully.
/// <para>
///   Also notifies the Support role group so all online support agents
///   immediately see newly assigned work without polling.
/// </para>
/// </summary>
public sealed class AssignRequestNotificationBehavior(
    IPublisher publisher,
    IRequestsService requestsService,
    IUserRepository userRepository,
    ILogger<AssignRequestNotificationBehavior> logger)
    : IPipelineBehavior<AssignRequestCommand, Unit>
{
    public async Task<Unit> Handle(
        AssignRequestCommand request,
        RequestHandlerDelegate<Unit> next,
        CancellationToken cancellationToken)
    {
        var result = await next();

        try
        {
            // Run both lookups concurrently — no inter-dependency.
            var idsTask = requestsService.GetRespectiveIds(request.RequestId);
            var assigneeTask = userRepository.GetByIdAsync(request.Dto.AssignToId);

            await Task.WhenAll(idsTask, assigneeTask);

            var ids = await idsTask;
            var assignee = await assigneeTask;

            if (ids is not null)
            {
                logger.LogInformation(
                    "Triggering RequestAssignedEvent for RequestId={RequestId}, AssignedToId={AssignedToId}",
                    ids.RequestId, request.Dto.AssignToId);

                await publisher.Publish(new RequestAssignedEvent(
                    RequestId: ids.RequestId,
                    AssignedToId: request.Dto.AssignToId,
                    AssignedToName: assignee?.FullName ?? string.Empty,
                    EmployeeId: ids.EmployeeId,
                    ManagerId: ids.ManagerId),
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to publish RequestAssignedEvent for RequestId={RequestId} AssignedToId={AssignedToId}",
                request.RequestId, request.Dto.AssignToId);
        }

        return result;
    }
}

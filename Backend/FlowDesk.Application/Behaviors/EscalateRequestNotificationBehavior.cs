using FlowDesk.Application.Events;
using FlowDesk.Application.Features.Requests.Commands;
using FlowDesk.Application.Features.Requests.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Behaviors;

/// <summary>
/// Pipeline behavior that publishes <see cref="RequestEscalatedEvent"/>
/// AFTER <see cref="EscalateRequestCommandHandler"/> completes successfully.
/// <para>
///   Design notes:
///   <list type="bullet">
///     <item>Runs entirely post-execution (<c>await next()</c> first) so the
///     DB write is committed before any SignalR broadcast is attempted.</item>
///     <item>Exceptions during event publishing are caught and logged. A
///     SignalR failure will NEVER roll back the already-committed escalation.</item>
///     <item>Both <c>GetRespectiveIds</c> lookups reuse the
///     <see cref="IRequestsService"/> already registered in DI — no extra
///     repository queries added to the handler itself.</item>
///   </list>
/// </para>
/// </summary>
public sealed class EscalateRequestNotificationBehavior(
    IPublisher publisher,
    IRequestsService requestsService,
    ILogger<EscalateRequestNotificationBehavior> logger)
    : IPipelineBehavior<EscalateRequestCommand, RequestResponseDto>
{
    public async Task<RequestResponseDto> Handle(
        EscalateRequestCommand request,
        RequestHandlerDelegate<RequestResponseDto> next,
        CancellationToken cancellationToken)
    {
        // ── Execute the existing handler first — business logic is untouched ──
        var result = await next();

        try
        {
            // Resolve stakeholder IDs from the service used by all other behaviors
            var ids = await requestsService.GetRespectiveIds(request.RequestId);

            if (ids is not null)
            {
                logger.LogInformation(
                    "Publishing RequestEscalatedEvent for RequestId={RequestId} " +
                    "EscalatedBy={AdminUserId}",
                    ids.RequestId, request.AdminUserId);

                await publisher.Publish(
                    new RequestEscalatedEvent(
                        RequestId:    ids.RequestId,
                        EmployeeId:   ids.EmployeeId,
                        ManagerId:    ids.ManagerId,
                        AssignedToId: ids.AssignedToId,
                        EscalatedById: request.AdminUserId),
                    cancellationToken);
            }
            else
            {
                logger.LogWarning(
                    "GetRespectiveIds returned null for RequestId={RequestId}; " +
                    "RequestEscalatedEvent will not be published.",
                    request.RequestId);
            }
        }
        catch (Exception ex)
        {
            // Real-time is best-effort — the DB write already succeeded.
            logger.LogError(ex,
                "Failed to publish RequestEscalatedEvent for RequestId={RequestId}. " +
                "The escalation was committed to the database; only the real-time " +
                "notification failed.",
                request.RequestId);
        }

        return result;
    }
}

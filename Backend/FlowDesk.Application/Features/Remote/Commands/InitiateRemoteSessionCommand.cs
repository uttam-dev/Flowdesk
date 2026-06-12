using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Common.Interfaces;
using FlowDesk.Application.Features.Remote.DTOs;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FlowDesk.Application.Features.Remote.Commands;

public record InitiateRemoteSessionCommand(
    int RequestId,
    int InitiatedByUserId       // injected from JWT claims in controller
) : IRequest<RemoteSessionDto>;

// ── Handler ───────────────────────────────────────────────────────────────────

public class InitiateRemoteSessionCommandHandler(
    IRemoteSessionRepository _remoteSessions,
    IRequestRepository _requests,
    IRealtimeService _realtime,
    ILogger<InitiateRemoteSessionCommandHandler> logger)
    : IRequestHandler<InitiateRemoteSessionCommand, RemoteSessionDto>
{
    public async Task<RemoteSessionDto> Handle(
        InitiateRemoteSessionCommand cmd,
        CancellationToken ct)
    {
        logger.LogInformation("Starting {Operation} with {@Command}", nameof(InitiateRemoteSessionCommandHandler), cmd);

        // 1. Validate request exists
        logger.LogInformation("Fetching Request with Id {RequestId}", cmd.RequestId);

        var request = await _requests.GetByIdAsync(cmd.RequestId);

        if (request == null)
        {
            logger.LogWarning("Request not found with Id {RequestId}", cmd.RequestId);
            throw new NotFoundException($"Request {cmd.RequestId} not found.");
        }

        if (request.AssignedToId != cmd.InitiatedByUserId)
        {
            logger.LogWarning("Unauthorized remote session attempt by UserId {UserId} for RequestId {RequestId}",
                cmd.InitiatedByUserId, cmd.RequestId);

            throw new ForbiddenException("Only the assigned support user can request remote access.");
        }

        if (request.Status == RequestStatusEnum.Resolved || request.Status == RequestStatusEnum.Closed)
        {
            logger.LogWarning("Remote session not allowed for RequestId {RequestId} with Status {Status}",
                cmd.RequestId, request.Status);

            throw new BadRequestException("Cannot start remote session on a resolved or closed request.");
        }

        // 2. Check active session
        logger.LogInformation("Checking existing active session for RequestId {RequestId}", cmd.RequestId);

        var alreadyActive = await _remoteSessions.HasActiveSessionAsync(cmd.RequestId, ct);

        if (alreadyActive)
        {
            logger.LogWarning("Active remote session already exists for RequestId {RequestId}", cmd.RequestId);
            throw new BadRequestException("A remote session is already pending or active for this request.");
        }

        // 3. Create session
        logger.LogInformation("Creating remote session for RequestId {RequestId}", cmd.RequestId);

        var session = new RemoteSession
        {
            RequestId = cmd.RequestId,
            InitiatedByUserId = cmd.InitiatedByUserId,
            TargetUserId = request.EmployeeId,
            Status = RemoteSessionStatusEnum.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdSession = await _remoteSessions.AddAsync(session, ct);
    

        logger.LogInformation("Remote session created with SessionId {SessionId} for RequestId {RequestId}",
            createdSession!.RemoteSessionId, createdSession.RequestId);

        // 4. Notify user
        logger.LogInformation("Sending real-time notification to TargetUserId {TargetUserId} for SessionId {SessionId}",
            createdSession.TargetUserId, createdSession.RemoteSessionId);

        await _realtime.NotifyRemoteSessionInitiatedAsync(
            targetUserId: createdSession.TargetUserId,
            sessionId: createdSession.RemoteSessionId,
            requestId: createdSession.RequestId,
            supportName: request.AssignedUser?.FullName ?? "Support");

        logger.LogInformation("Completed {Operation} for RequestId {RequestId} and SessionId {SessionId}",
            nameof(InitiateRemoteSessionCommandHandler), createdSession.RequestId, createdSession.RemoteSessionId);

        return MapToDto(createdSession, request);
    }

    private static RemoteSessionDto MapToDto(RemoteSession s, Domain.Entities.Request r) => new()
    {
        RemoteSessionId = s.RemoteSessionId,
        RequestId = s.RequestId,
        InitiatedByUserId = s.InitiatedByUserId,
        InitiatedByName = r.AssignedUser?.FullName ?? string.Empty,
        TargetUserId = s.TargetUserId,
        TargetUserName = r.Employee?.FullName ?? string.Empty,
        Status = s.Status.ToString(),
        CreatedAt = s.CreatedAt
    };
}

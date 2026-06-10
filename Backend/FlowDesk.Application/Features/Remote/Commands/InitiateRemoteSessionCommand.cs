using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Common.Interfaces;
using FlowDesk.Application.Features.Remote.DTOs;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Interfaces;
using MediatR;

namespace FlowDesk.Application.Features.Remote.Commands;

public record InitiateRemoteSessionCommand(
    int RequestId,
    int InitiatedByUserId       // injected from JWT claims in controller
) : IRequest<RemoteSessionDto>;

// ── Handler ───────────────────────────────────────────────────────────────────

public class InitiateRemoteSessionCommandHandler(IRemoteSessionRepository _remoteSessions,
        IRequestRepository _requests,
        IRealtimeService _realtime)
    : IRequestHandler<InitiateRemoteSessionCommand, RemoteSessionDto>
{

    public async Task<RemoteSessionDto> Handle(
        InitiateRemoteSessionCommand cmd,
        CancellationToken ct)
    {
        // 1. Validate request exists and is assigned to this support user
        var request = await _requests.GetByIdAsync(cmd.RequestId)
            ?? throw new NotFoundException($"Request {cmd.RequestId} not found.");

        if (request.AssignedToId != cmd.InitiatedByUserId)
            throw new ForbiddenException("Only the assigned support user can request remote access.");

        if (request.Status == RequestStatusEnum.Resolved || request.Status == RequestStatusEnum.Closed)
            throw new BadRequestException("Cannot start remote session on a resolved or closed request.");

        // 2. Guard — no duplicate active session
        var alreadyActive = await _remoteSessions.HasActiveSessionAsync(cmd.RequestId, ct);
        if (alreadyActive)
            throw new BadRequestException("A remote session is already pending or active for this request.");

        // 3. Create session
        var session = new RemoteSession
        {
            RequestId = cmd.RequestId,
            InitiatedByUserId = cmd.InitiatedByUserId,
            TargetUserId = request.EmployeeId,   // employee / manager who raised the request
            Status = RemoteSessionStatusEnum.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _remoteSessions.AddAsync(session, ct);

        // 4. Notify the target user in real-time (they see Accept/Reject popup)
        await _realtime.NotifyRemoteSessionInitiatedAsync(
            targetUserId: session.TargetUserId,
            sessionId: session.RemoteSessionId,
            requestId: session.RequestId,
            supportName: request.AssignedUser?.FullName ?? "Support");

        return MapToDto(session, request);
    }

    private static RemoteSessionDto MapToDto(RemoteSession s, Domain.Entities.Request r) => new()
    {
        Id = s.RemoteSessionId,
        RequestId = s.RequestId,
        InitiatedByUserId = s.InitiatedByUserId,
        InitiatedByName = r.AssignedUser?.FullName ?? string.Empty,
        TargetUserId = s.TargetUserId,
        TargetUserName = r.Employee?.FullName ?? string.Empty,
        Status = s.Status.ToString(),
        CreatedAt = s.CreatedAt
    };
}

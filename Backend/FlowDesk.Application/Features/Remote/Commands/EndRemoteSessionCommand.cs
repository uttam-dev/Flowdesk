using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Common.Interfaces;
using FlowDesk.Application.Features.Remote.DTOs;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Interfaces;
using MediatR;

namespace FlowDesk.Application.Features.Remote.Commands
{


    public record EndRemoteSessionCommand(
        int SessionId,
        int EndedByUserId,
        string? ResolutionNotes,
        bool ResolveRequest
    ) : IRequest<RemoteSessionDto>;

    // ── Handler ───────────────────────────────────────────────────────────────────

    public class EndRemoteSessionCommandHandler(
            IRemoteSessionRepository _remoteSessions,
            IRequestRepository _requests,
            IRealtimeService _realtime, IRequestHistoryRepository reqHistory)
        : IRequestHandler<EndRemoteSessionCommand, RemoteSessionDto>
    {

        public async Task<RemoteSessionDto> Handle(
            EndRemoteSessionCommand cmd,
            CancellationToken ct)
        {
            var session = await _remoteSessions.GetByIdAsync(cmd.SessionId, ct)
                ?? throw new NotFoundException($"Remote session {cmd.SessionId} not found.");

            if (session.InitiatedByUserId != cmd.EndedByUserId)
                throw new ForbiddenException("Only the support user who started this session can end it.");

            if (session.Status != RemoteSessionStatusEnum.Active &&
                session.Status != RemoteSessionStatusEnum.Accepted)
                throw new BadRequestException($"Session is already {session.Status}.");

            // 1. Close the session
            var endedAt = DateTime.UtcNow;
            session.Status = RemoteSessionStatusEnum.Ended;
            session.EndedAt = endedAt;
            session.ResolutionNotes = cmd.ResolutionNotes;
            session.UpdatedAt = endedAt;

            // Calculate duration only if session actually went live
            if (session.StartedAt.HasValue)
                session.DurationSeconds = (int)(endedAt - session.StartedAt.Value).TotalSeconds;

            await _remoteSessions.UpdateAsync(session, ct);

            // 2. Optionally resolve the parent request
            if (cmd.ResolveRequest)
            {
                var request = await _requests.GetByIdAsync(session.RequestId);
                var reqHistoryEntry = new RequestHistory
                {
                    RequestId = session.RequestId,
                    ChangedById = cmd.EndedByUserId,
                    OldStatus = request?.Status,
                    NewStatus = RequestStatusEnum.Resolved,
                    ChangedOn = endedAt,
                };

                if (request is not null && request.Status != RequestStatusEnum.Resolved)
                {
                    request.Status = RequestStatusEnum.Resolved;
                    request.UpdatedOn = endedAt;
                    await _requests.Update(request);
                    await reqHistory.AddAsync(reqHistoryEntry);

                    //close request if it's not closed yet by system
                    reqHistoryEntry.OldStatus = reqHistoryEntry.NewStatus;
                    reqHistoryEntry.NewStatus = RequestStatusEnum.Closed;
                    reqHistoryEntry.IsSystemGenerated = true;
                    reqHistoryEntry.ChangedById = null;
                    await reqHistory.AddAsync(reqHistoryEntry);
                }
            }

            // 3. Notify both sides the session has ended
            await _realtime.NotifyRemoteSessionEndedAsync(
                targetUserId: session.TargetUserId,
                supportUserId: session.InitiatedByUserId,
                sessionId: session.RemoteSessionId,
                resolved: cmd.ResolveRequest);

            return new RemoteSessionDto
            {
                Id = session.RemoteSessionId,
                RequestId = session.RequestId,
                InitiatedByUserId = session.InitiatedByUserId,
                InitiatedByName = session.InitiatedBy?.FullName ?? string.Empty,
                TargetUserId = session.TargetUserId,
                TargetUserName = session.TargetUser?.FullName ?? string.Empty,
                Status = session.Status.ToString(),
                StartedAt = session.StartedAt,
                EndedAt = session.EndedAt,
                DurationSeconds = session.DurationSeconds,
                ResolutionNotes = session.ResolutionNotes,
                CreatedAt = session.CreatedAt
            };
        }
    }

};
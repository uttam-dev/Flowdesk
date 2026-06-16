using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Common.Interfaces;
using FlowDesk.Application.Features.Remote.DTOs;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

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
         IRealtimeService _realtime,
         IRequestHistoryRepository reqHistory,
         ILogger<EndRemoteSessionCommandHandler> logger)
     : IRequestHandler<EndRemoteSessionCommand, RemoteSessionDto>
    {
        public async Task<RemoteSessionDto> Handle(
            EndRemoteSessionCommand cmd,
            CancellationToken ct)
        {
            logger.LogInformation("Starting {Operation} with {@Command}",
                nameof(EndRemoteSessionCommand), cmd);

            logger.LogInformation("Fetching RemoteSession with SessionId {SessionId}", cmd.SessionId);

            var session = await _remoteSessions.GetByIdAsync(cmd.SessionId, ct);

            if (session == null)
            {
                logger.LogWarning("RemoteSession not found with SessionId {SessionId}", cmd.SessionId);
                throw new NotFoundException($"Remote session {cmd.SessionId} not found.");
            }

            if (session.InitiatedByUserId != cmd.EndedByUserId)
            {
                logger.LogWarning(
                    "Unauthorized end attempt by UserId {UserId} for SessionId {SessionId}",
                    cmd.EndedByUserId, cmd.SessionId);

                throw new ForbiddenException("Only the support user who started this session can end it.");
            }

            if (session.Status != RemoteSessionStatusEnum.Active &&
                session.Status != RemoteSessionStatusEnum.Accepted)
            {
                logger.LogWarning(
                    "Invalid session end attempt for SessionId {SessionId} with Status {Status}",
                    cmd.SessionId, session.Status);

                throw new BadRequestException($"Session is already {session.Status}.");
            }

            // 1. Close the session
            logger.LogInformation("Ending RemoteSession with SessionId {SessionId}", cmd.SessionId);

            var endedAt = DateTime.UtcNow;
            session.Status = RemoteSessionStatusEnum.Ended;
            session.EndedAt = endedAt;
            session.ResolutionNotes = cmd.ResolutionNotes;
            session.UpdatedAt = endedAt;

            if (session.StartedAt.HasValue)
            {
                session.DurationSeconds = (int)(endedAt - session.StartedAt.Value).TotalSeconds;

                logger.LogInformation(
                    "Session duration calculated for SessionId {SessionId}: {DurationSeconds} seconds",
                    session.RemoteSessionId, session.DurationSeconds);
            }

            await _remoteSessions.UpdateAsync(session, ct);

            logger.LogInformation(
                "RemoteSession ended successfully with SessionId {SessionId}",
                session.RemoteSessionId);

            // 2. Optionally resolve the parent request
            if (cmd.ResolveRequest)
            {
                logger.LogInformation(
                    "Resolving RequestId {RequestId} for SessionId {SessionId}",
                    session.RequestId, session.RemoteSessionId);

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

                    logger.LogInformation(
                        "Request {RequestId} marked as Resolved",
                        session.RequestId);

                    reqHistoryEntry.OldStatus = reqHistoryEntry.NewStatus;
                    reqHistoryEntry.NewStatus = RequestStatusEnum.Closed;
                    reqHistoryEntry.IsSystemGenerated = true;
                    reqHistoryEntry.ChangedById = null;

                    await reqHistory.AddAsync(reqHistoryEntry);

                    logger.LogInformation(
                        "Request {RequestId} marked as Closed by system",
                        session.RequestId);
                }
            }

            // 3. Notify both sides
            logger.LogInformation(
                "Sending end notification for SessionId {SessionId} to TargetUserId {TargetUserId} and SupportUserId {SupportUserId}",
                session.RemoteSessionId, session.TargetUserId, session.InitiatedByUserId);

            await _realtime.NotifyRemoteSessionEndedAsync(
                targetUserId: session.TargetUserId,
                supportUserId: session.InitiatedByUserId,
                sessionId: session.RemoteSessionId,
                resolved: cmd.ResolveRequest);

            logger.LogInformation(
                "Completed {Operation} for SessionId {SessionId}",
                nameof(EndRemoteSessionCommand), session.RemoteSessionId);

            return new RemoteSessionDto
            {
                RemoteSessionId = session.RemoteSessionId,
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
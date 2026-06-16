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

    public record RespondToRemoteSessionCommand(
        int SessionId,
        int RespondingUserId,       // injected from JWT claims
        bool Accepted,
        string? RejectionReason
    ) : IRequest<RemoteSessionDto>;


    public class RespondToRemoteSessionCommandHandler(
            IRemoteSessionRepository _remoteSessions,
            IRealtimeService _realtime,
            ILogger<RespondToRemoteSessionCommandHandler> logger)
        : IRequestHandler<RespondToRemoteSessionCommand, RemoteSessionDto>
    {
        public async Task<RemoteSessionDto> Handle(
            RespondToRemoteSessionCommand cmd,
            CancellationToken ct)
        {
            logger.LogInformation("Starting {Operation} with {@Command}",
                nameof(RespondToRemoteSessionCommand), cmd);

            logger.LogInformation("Fetching RemoteSession with SessionId {SessionId}", cmd.SessionId);

            var session = await _remoteSessions.GetByIdAsync(cmd.SessionId, ct);

            if (session == null)
            {
                logger.LogWarning("RemoteSession not found with SessionId {SessionId}", cmd.SessionId);
                throw new NotFoundException($"Remote session {cmd.SessionId} not found.");
            }

            if (session.TargetUserId != cmd.RespondingUserId)
            {
                logger.LogWarning(
                    "Unauthorized response attempt by UserId {UserId} for SessionId {SessionId}",
                    cmd.RespondingUserId, cmd.SessionId);

                throw new ForbiddenException("You are not the target of this remote session request.");
            }

            if (session.Status != RemoteSessionStatusEnum.Pending)
            {
                logger.LogWarning(
                    "Invalid response attempt for SessionId {SessionId} with Status {Status}",
                    cmd.SessionId, session.Status);

                throw new BadRequestException($"Session is already {session.Status}. Cannot respond again.");
            }

            if (cmd.Accepted)
            {
                logger.LogInformation(
                    "Accepting RemoteSession with SessionId {SessionId} by UserId {UserId}",
                    cmd.SessionId, cmd.RespondingUserId);

                session.Status = RemoteSessionStatusEnum.Accepted;
                session.UpdatedAt = DateTime.UtcNow;

                await _remoteSessions.UpdateAsync(session, ct);

                logger.LogInformation(
                    "RemoteSession accepted for SessionId {SessionId}",
                    session.RemoteSessionId);

                await _realtime.NotifyRemoteSessionAcceptedAsync(
                    supportUserId: session.InitiatedByUserId,
                    targetUserId: session.TargetUserId,
                    sessionId: session.RemoteSessionId);

                logger.LogInformation(
                    "Acceptance notification sent for SessionId {SessionId} to SupportUserId {SupportUserId}",
                    session.RemoteSessionId, session.InitiatedByUserId);
            }
            else
            {
                logger.LogInformation(
                    "Rejecting RemoteSession with SessionId {SessionId} by UserId {UserId}",
                    cmd.SessionId, cmd.RespondingUserId);

                session.Status = RemoteSessionStatusEnum.Rejected;
                session.RejectionReason = cmd.RejectionReason;
                session.UpdatedAt = DateTime.UtcNow;

                await _remoteSessions.UpdateAsync(session, ct);

                logger.LogInformation(
                    "RemoteSession rejected for SessionId {SessionId} with Reason {Reason}",
                    session.RemoteSessionId, cmd.RejectionReason);

                await _realtime.NotifyRemoteSessionRejectedAsync(
                    supportUserId: session.InitiatedByUserId,
                    sessionId: session.RemoteSessionId,
                    rejectionReason: cmd.RejectionReason);

                logger.LogInformation(
                    "Rejection notification sent for SessionId {SessionId} to SupportUserId {SupportUserId}",
                    session.RemoteSessionId, session.InitiatedByUserId);
            }

            logger.LogInformation(
                "Completed {Operation} for SessionId {SessionId}",
                nameof(RespondToRemoteSessionCommand), session.RemoteSessionId);

            return MapToDto(session);
        }

        private static RemoteSessionDto MapToDto(RemoteSession s) => new()
        {
            RemoteSessionId = s.RemoteSessionId,
            RequestId = s.RequestId,
            InitiatedByUserId = s.InitiatedByUserId,
            InitiatedByName = s.InitiatedBy?.FullName ?? string.Empty,
            TargetUserId = s.TargetUserId,
            TargetUserName = s.TargetUser?.FullName ?? string.Empty,
            Status = s.Status.ToString(),
            RejectionReason = s.RejectionReason,
            CreatedAt = s.CreatedAt
        };
    }

};
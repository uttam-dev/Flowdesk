using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Features.Remote.DTOs;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Remote.Commands
{


    public record ActivateRemoteSessionCommand(
        int SessionId,
        int CalledByUserId      // must be the target user (agent side)
    ) : IRequest<RemoteSessionDto>;

    public class ActivateRemoteSessionCommandHandler(
        IRemoteSessionRepository _remoteSessions,
        ILogger<ActivateRemoteSessionCommandHandler> logger)
        : IRequestHandler<ActivateRemoteSessionCommand, RemoteSessionDto>
    {
        public async Task<RemoteSessionDto> Handle(
            ActivateRemoteSessionCommand cmd,
            CancellationToken ct)
        {
            logger.LogInformation("Starting {Operation} with {@Command}",
                nameof(ActivateRemoteSessionCommandHandler), cmd);

            // Fetch session
            logger.LogInformation("Fetching RemoteSession with SessionId {SessionId}", cmd.SessionId);

            var session = await _remoteSessions.GetByIdAsync(cmd.SessionId, ct);

            if (session == null)
            {
                logger.LogWarning("RemoteSession not found with SessionId {SessionId}", cmd.SessionId);
                throw new NotFoundException($"Remote session {cmd.SessionId} not found.");
            }

            // Authorization check
            if (session.TargetUserId != cmd.CalledByUserId)
            {
                logger.LogWarning(
                    "Unauthorized activation attempt by UserId {UserId} for SessionId {SessionId}",
                    cmd.CalledByUserId, cmd.SessionId);

                throw new ForbiddenException("Only the target user's agent can activate the session.");
            }

            // State validation
            if (session.Status != RemoteSessionStatusEnum.Accepted)
            {
                logger.LogWarning(
                    "Invalid session state transition for SessionId {SessionId}. CurrentStatus {Status}",
                    cmd.SessionId, session.Status);

                throw new BadRequestException(
                    $"Session must be in Accepted state to activate. Current: {session.Status}");
            }

            // Activate session
            logger.LogInformation("Activating RemoteSession with SessionId {SessionId}", cmd.SessionId);

            session.Status = RemoteSessionStatusEnum.Active;
            session.StartedAt = DateTime.UtcNow;
            session.UpdatedAt = DateTime.UtcNow;

            await _remoteSessions.UpdateAsync(session, ct);

            logger.LogInformation(
                "RemoteSession activated successfully with SessionId {SessionId} at {StartedAt}",
                session.RemoteSessionId, session.StartedAt);

            logger.LogInformation(
                "Completed {Operation} for SessionId {SessionId}",
                nameof(ActivateRemoteSessionCommandHandler), session.RemoteSessionId);

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
                CreatedAt = session.CreatedAt
            };
        }
    }

};
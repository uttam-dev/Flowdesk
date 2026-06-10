using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Features.Remote.DTOs;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Interfaces;
using MediatR;

namespace FlowDesk.Application.Features.Remote.Commands
{


    public record ActivateRemoteSessionCommand(
        int SessionId,
        int CalledByUserId      // must be the target user (agent side)
    ) : IRequest<RemoteSessionDto>;

    public class ActivateRemoteSessionCommandHandler(IRemoteSessionRepository _remoteSessions)
        : IRequestHandler<ActivateRemoteSessionCommand, RemoteSessionDto>
    {

        public async Task<RemoteSessionDto> Handle(
            ActivateRemoteSessionCommand cmd,
            CancellationToken ct)
        {
            var session = await _remoteSessions.GetByIdAsync(cmd.SessionId, ct)
                ?? throw new NotFoundException($"Remote session {cmd.SessionId} not found.");

            if (session.TargetUserId != cmd.CalledByUserId)
                throw new ForbiddenException("Only the target user's agent can activate the session.");

            if (session.Status != RemoteSessionStatusEnum.Accepted)
                throw new BadRequestException($"Session must be in Accepted state to activate. Current: {session.Status}");

            session.Status = RemoteSessionStatusEnum.Active;
            session.StartedAt = DateTime.UtcNow;
            session.UpdatedAt = DateTime.UtcNow;

            await _remoteSessions.UpdateAsync(session, ct);

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
                CreatedAt = session.CreatedAt
            };
        }
    }

};
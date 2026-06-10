using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Common.Interfaces;
using FlowDesk.Application.Features.Remote.DTOs;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Interfaces;
using MediatR;

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
            IRealtimeService _realtime)
        : IRequestHandler<RespondToRemoteSessionCommand, RemoteSessionDto>
    {

        public async Task<RemoteSessionDto> Handle(
            RespondToRemoteSessionCommand cmd,
            CancellationToken ct)
        {
            var session = await _remoteSessions.GetByIdAsync(cmd.SessionId, ct)
                ?? throw new NotFoundException($"Remote session {cmd.SessionId} not found.");

            // Only the intended target can respond
            if (session.TargetUserId != cmd.RespondingUserId)
                throw new ForbiddenException("You are not the target of this remote session request.");

            if (session.Status != RemoteSessionStatusEnum.Pending)
                throw new BadRequestException($"Session is already {session.Status}. Cannot respond again.");

            if (cmd.Accepted)
            {
                // Accepted — WebRTC handshake will start via SignalR after this
                session.Status = RemoteSessionStatusEnum.Accepted;
                session.UpdatedAt = DateTime.UtcNow;

                await _remoteSessions.UpdateAsync(session, ct);

                // Tell the support user the target accepted — triggers WebRTC offer from agent
                await _realtime.NotifyRemoteSessionAcceptedAsync(
                    supportUserId: session.InitiatedByUserId,
                    sessionId: session.RemoteSessionId);
            }
            else
            {
                // Rejected
                session.Status = RemoteSessionStatusEnum.Rejected;
                session.RejectionReason = cmd.RejectionReason;
                session.UpdatedAt = DateTime.UtcNow;

                await _remoteSessions.UpdateAsync(session, ct);

                // Notify support user
                await _realtime.NotifyRemoteSessionRejectedAsync(
                    supportUserId: session.InitiatedByUserId,
                    sessionId: session.RemoteSessionId,
                    rejectionReason: cmd.RejectionReason);
            }

            return MapToDto(session);
        }

        private static RemoteSessionDto MapToDto(RemoteSession s) => new()
        {
            Id = s.RemoteSessionId,
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
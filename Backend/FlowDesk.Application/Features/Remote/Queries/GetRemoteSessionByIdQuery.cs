using AutoMapper;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Features.Remote.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Remote.Queries
{
    public record GetRemoteSessionByIdQuery(int SessionId) : IRequest<RemoteSessionDto>;

    public class GetRemoteSessionByIdQueryHandler(
         IRemoteSessionRepository _remoteSessions,
         IMapper _mapper,
         ILogger<GetRemoteSessionByIdQueryHandler> logger)
     : IRequestHandler<GetRemoteSessionByIdQuery, RemoteSessionDto>
    {
        public async Task<RemoteSessionDto> Handle(
            GetRemoteSessionByIdQuery query,
            CancellationToken ct)
        {
            logger.LogInformation("Starting {Operation} with {@Query}",
                nameof(GetRemoteSessionByIdQuery), query);

            logger.LogInformation(
                "Fetching RemoteSession with SessionId {SessionId}",
                query.SessionId);

            var session = await _remoteSessions.GetByIdAsync(query.SessionId, ct);

            if (session == null)
            {
                logger.LogWarning(
                    "RemoteSession not found with SessionId {SessionId}",
                    query.SessionId);

                throw new NotFoundException($"Remote session {query.SessionId} not found.");
            }

            logger.LogInformation(
                "RemoteSession found with SessionId {SessionId}",
                session.RemoteSessionId);

            var result = _mapper.Map<RemoteSessionDto>(session);

            logger.LogInformation(
                "Completed {Operation} for SessionId {SessionId}",
                nameof(GetRemoteSessionByIdQuery), session.RemoteSessionId);

            return result;
        }
    }
}
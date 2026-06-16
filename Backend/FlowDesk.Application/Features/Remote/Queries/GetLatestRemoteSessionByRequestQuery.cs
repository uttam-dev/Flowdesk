using AutoMapper;
using FlowDesk.Application.Features.Remote.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Remote.Queries
{
    public record GetLatestRemoteSessionByRequestQuery(int RequestId) : IRequest<RemoteSessionDto?>;
    public class GetLatestRemoteSessionByRequestQueryHandler(
            IRemoteSessionRepository _remoteSessions,
            IMapper _mapper,
            ILogger<GetLatestRemoteSessionByRequestQueryHandler> logger)
        : IRequestHandler<GetLatestRemoteSessionByRequestQuery, RemoteSessionDto?>
    {
        public async Task<RemoteSessionDto?> Handle(
            GetLatestRemoteSessionByRequestQuery query,
            CancellationToken ct)
        {
            logger.LogInformation("Starting {Operation} with {@Query}",
                nameof(GetLatestRemoteSessionByRequestQuery), query);

            logger.LogInformation(
                "Fetching latest RemoteSession for RequestId {RequestId}",
                query.RequestId);

            var session = await _remoteSessions.GetLatestByRequestIdAsync(query.RequestId, ct);

            if (session == null)
            {
                logger.LogInformation(
                    "No RemoteSession found for RequestId {RequestId}",
                    query.RequestId);

                return null;
            }

            logger.LogInformation(
                "Latest RemoteSession found with SessionId {SessionId} for RequestId {RequestId}",
                session.RemoteSessionId, query.RequestId);

            var result = _mapper.Map<RemoteSessionDto>(session);

            logger.LogInformation(
                "Completed {Operation} for RequestId {RequestId}",
                nameof(GetLatestRemoteSessionByRequestQuery), query.RequestId);

            return result;
        }
    }
}

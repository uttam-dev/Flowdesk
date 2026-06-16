using AutoMapper;
using FlowDesk.Application.Features.Remote.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Remote.Queries
{
    public record GetRemoteSessionHistoryQuery(int RequestId) : IRequest<IReadOnlyList<RemoteSessionDto>>;

    public class GetRemoteSessionHistoryQueryHandler(
            IRemoteSessionRepository _remoteSessions,
            IMapper _mapper,
            ILogger<GetRemoteSessionHistoryQueryHandler> logger)
        : IRequestHandler<GetRemoteSessionHistoryQuery, IReadOnlyList<RemoteSessionDto>>
    {
        public async Task<IReadOnlyList<RemoteSessionDto>> Handle(
            GetRemoteSessionHistoryQuery query,
            CancellationToken ct)
        {
            logger.LogInformation("Starting {Operation} with {@Query}",
                nameof(GetRemoteSessionHistoryQuery), query);

            logger.LogInformation(
                "Fetching RemoteSession history for RequestId {RequestId}",
                query.RequestId);

            var sessions = await _remoteSessions.GetAllByRequestIdAsync(query.RequestId, ct);

            if (sessions == null || !sessions.Any())
            {
                logger.LogInformation(
                    "No RemoteSession history found for RequestId {RequestId}",
                    query.RequestId);

                return Array.Empty<RemoteSessionDto>();
            }

            logger.LogInformation(
                "Found {Count} RemoteSessions for RequestId {RequestId}",
                sessions.Count(), query.RequestId);

            var result = sessions
                .Select(s => _mapper.Map<RemoteSessionDto>(s))
                .ToList();

            logger.LogInformation(
                "Completed {Operation} for RequestId {RequestId}",
                nameof(GetRemoteSessionHistoryQuery), query.RequestId);

            return result;
        }
    }
}

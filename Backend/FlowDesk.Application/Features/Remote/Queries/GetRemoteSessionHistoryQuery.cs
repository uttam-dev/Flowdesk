using AutoMapper;
using FlowDesk.Application.Features.Remote.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;

namespace FlowDesk.Application.Features.Remote.Queries
{
    public record GetRemoteSessionHistoryQuery(int RequestId) : IRequest<IReadOnlyList<RemoteSessionDto>>;

    public class GetRemoteSessionHistoryQueryHandler(IRemoteSessionRepository _remoteSessions, IMapper _mapper)
        : IRequestHandler<GetRemoteSessionHistoryQuery, IReadOnlyList<RemoteSessionDto>>
    {
        public async Task<IReadOnlyList<RemoteSessionDto>> Handle(
            GetRemoteSessionHistoryQuery query,
            CancellationToken ct)
        {
            var sessions = await _remoteSessions.GetAllByRequestIdAsync(query.RequestId, ct);
            return sessions.Select(s => _mapper.Map<RemoteSessionDto>(s)).ToList();
        }
    }
}

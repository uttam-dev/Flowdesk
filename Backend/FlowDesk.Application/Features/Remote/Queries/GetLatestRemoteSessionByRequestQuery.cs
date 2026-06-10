using AutoMapper;
using FlowDesk.Application.Features.Remote.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;

namespace FlowDesk.Application.Features.Remote.Queries
{
    public record GetLatestRemoteSessionByRequestQuery(int RequestId) : IRequest<RemoteSessionDto?>;

    public class GetLatestRemoteSessionByRequestQueryHandler(IRemoteSessionRepository _remoteSessions, IMapper _mapper)
        : IRequestHandler<GetLatestRemoteSessionByRequestQuery, RemoteSessionDto?>
    {

        public async Task<RemoteSessionDto?> Handle(
            GetLatestRemoteSessionByRequestQuery query,
            CancellationToken ct)
        {
            var session = await _remoteSessions.GetLatestByRequestIdAsync(query.RequestId, ct);
            return session is null ? null : _mapper.Map<RemoteSessionDto>(session);
        }
    }
}

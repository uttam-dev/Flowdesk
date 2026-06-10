using AutoMapper;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Features.Remote.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;

namespace FlowDesk.Application.Features.Remote.Queries
{
    public record GetRemoteSessionByIdQuery(int SessionId) : IRequest<RemoteSessionDto>;

    public class GetRemoteSessionByIdQueryHandler(IRemoteSessionRepository _remoteSessions, IMapper _mapper)
        : IRequestHandler<GetRemoteSessionByIdQuery, RemoteSessionDto>
    {

        public async Task<RemoteSessionDto> Handle(
            GetRemoteSessionByIdQuery query,
            CancellationToken ct)
        {
            var session = await _remoteSessions.GetByIdAsync(query.SessionId, ct)
                ?? throw new NotFoundException($"Remote session {query.SessionId} not found.");

            return _mapper.Map<RemoteSessionDto>(session);
        }
    }
}
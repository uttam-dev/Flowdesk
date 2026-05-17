using FlowDesk.Domain.DTOs;
using MediatR;
using FlowDesk.Application.Features.Requests.DTOs;
using AutoMapper;
using FlowDesk.Domain.Interfaces;

namespace FlowDesk.Application.Features.Requests.Queries
{
    public record GetAllTeamRequestsQuery(int currentUserId, FilterRequestQueryDto Query) : IRequest<PagedResult<RequestResponseDto>>;

    public class GetAllTeamRequestsQueryHandler(IRequestRepository requestRepository, IMapper mapper) : IRequestHandler<GetAllTeamRequestsQuery, PagedResult<RequestResponseDto>>
    {
        public async Task<PagedResult<RequestResponseDto>> Handle(GetAllTeamRequestsQuery request, CancellationToken cancellationToken)
        {
            var (totalCount, requestsResult) = await requestRepository.GetAllTeamRequestsAsync(request.Query, request.currentUserId);
            var requestsResponse = mapper.Map<List<RequestResponseDto>>(requestsResult);
            return new PagedResult<RequestResponseDto>
            {
                Items = requestsResponse,
                PageNumber = request.Query.PageNumber,
                PageSize = request.Query.PageSize,
                TotalCount = totalCount
            };
        }
    }
}

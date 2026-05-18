using AutoMapper;
using FlowDesk.Application.Features.Requests.DTOs;
using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;

namespace FlowDesk.Application.Features.Requests.Queries
{
    public record GetAllRequestsQuery(FilterRequestQueryDto Ouery, int CurrentUserId, string Role) : IRequest<PagedResult<RequestResponseDto>>;

    public class GetAllRequestsQueryHandler(IRequestRepository requestRepository, IMapper mapper) : IRequestHandler<GetAllRequestsQuery, PagedResult<RequestResponseDto>>
    {
        public async Task<PagedResult<RequestResponseDto>> Handle(GetAllRequestsQuery request, CancellationToken cancellationToken)
        {
            var (totalCount, requestsResult) = await requestRepository.GetAllAsync(request.Ouery, request.CurrentUserId, request.Role);

            foreach (var reqResult in requestsResult)
            {
                if (reqResult!.Category!.IsApprovalRequired == false)
                {
                    reqResult.Employee?.Manager = null;
                }
            }

            var requestsResponse = mapper.Map<List<RequestResponseDto>>(requestsResult);

            return new PagedResult<RequestResponseDto>
            {
                Items = requestsResponse,
                PageNumber = request.Ouery.PageNumber,
                PageSize = request.Ouery.PageSize,
                TotalCount = totalCount
            };
        }
    }

}

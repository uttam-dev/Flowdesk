using AutoMapper;
using FlowDesk.Application.Features.Requests.DTOs;
using FlowDesk.Application.Services;
using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Requests.Queries
{
    public record GetAllRequestsQuery(FilterRequestQueryDto Ouery, int CurrentUserId, string Role) : IRequest<PagedResult<RequestResponseDto>>;

    public class GetAllRequestsQueryHandler(
    IRequestRepository requestRepository,
    IMapper mapper,
    ISlaService slaService,
    ILogger<GetAllRequestsQueryHandler> logger)
    : IRequestHandler<GetAllRequestsQuery, PagedResult<RequestResponseDto>>
    {
        public async Task<PagedResult<RequestResponseDto>> Handle(GetAllRequestsQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting {Operation} with {@Query} for UserId {UserId} and Role {Role}",
                nameof(GetAllRequestsQueryHandler), request.Ouery, request.CurrentUserId, request.Role);

            var (totalCount, requestsResult) =
                await requestRepository.GetAllAsync(request.Ouery, request.CurrentUserId, request.Role);

            logger.LogInformation("Fetched {Count} requests (TotalCount: {TotalCount})",
                requestsResult.Count(), totalCount);

            foreach (var reqResult in requestsResult)
            {
                if (reqResult!.Category!.IsApprovalRequired == false)
                {
                    reqResult.Employee?.Manager = null;
                }
            }

            logger.LogInformation("Mapping request entities to DTOs");

            var requestsResponse = mapper.Map<List<RequestResponseDto>>(requestsResult);
            foreach (var item in requestsResponse)
            {
                item.SlaStatus = slaService.CalculateSlaStatus(item.DueDate, item.Status.ToString());
            }

            logger.LogInformation("Completed {Operation}", nameof(GetAllRequestsQueryHandler));

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

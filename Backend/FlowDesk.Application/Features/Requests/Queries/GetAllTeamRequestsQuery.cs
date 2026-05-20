using AutoMapper;
using FlowDesk.Application.Features.Requests.DTOs;
using FlowDesk.Application.Services;
using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Requests.Queries
{
    public record GetAllTeamRequestsQuery(int currentUserId, FilterRequestQueryDto Query) : IRequest<PagedResult<RequestResponseDto>>;

    public class GetAllTeamRequestsQueryHandler(
     IRequestRepository requestRepository,
     IMapper mapper,
     ISlaService slaService,
     ILogger<GetAllTeamRequestsQueryHandler> logger)
     : IRequestHandler<GetAllTeamRequestsQuery, PagedResult<RequestResponseDto>>
    {
        public async Task<PagedResult<RequestResponseDto>> Handle(GetAllTeamRequestsQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting {Operation} with {@Query} for UserId {UserId}",
                nameof(GetAllTeamRequestsQueryHandler), request.Query, request.currentUserId);

            var (totalCount, requestsResult) =
                await requestRepository.GetAllTeamRequestsAsync(request.Query, request.currentUserId);

            logger.LogInformation("Fetched {Count} team requests (TotalCount: {TotalCount})",
                requestsResult.Count(), totalCount);

            logger.LogInformation("Mapping team requests to DTOs");

            var requestsResponse = mapper.Map<List<RequestResponseDto>>(requestsResult);
            foreach (var item in requestsResponse)
            {
                item.SlaStatus = slaService.CalculateSlaStatus(item.DueDate, item.Status.ToString());
            }

            logger.LogInformation("Completed {Operation}", nameof(GetAllTeamRequestsQueryHandler));

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

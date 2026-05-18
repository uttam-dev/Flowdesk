using FlowDesk.Application.Features.Requests.DTOs;
using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Requests.Queries
{
    public record GetRequestRemarksQuery(FilterRequestRemarksQueryDto Dto) : IRequest<List<RequestRemarkResponseDto>>;

    public class GetRequestRemarksQueryHandler(
       IRequestsService _requestsService,
       ILogger<GetRequestRemarksQueryHandler> logger)
       : IRequestHandler<GetRequestRemarksQuery, List<RequestRemarkResponseDto>>
    {
        public async Task<List<RequestRemarkResponseDto>> Handle(GetRequestRemarksQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting {Operation} with {@Request}",
                nameof(GetRequestRemarksQueryHandler), request.Dto);

            var remarksList = await _requestsService.GetRequestRemarksAsync(request.Dto);

            logger.LogInformation("Fetched {Count} remarks", remarksList.Count());

            var result = remarksList.Select(r => new RequestRemarkResponseDto
            {
                MasterRemarksId = r.MasterRemarksId,
                RemarksText = r.RemarksText,
                ActionType = (int)r.ActionType
            }).ToList();

            logger.LogInformation("Completed {Operation}", nameof(GetRequestRemarksQueryHandler));

            return result;
        }
    }
}

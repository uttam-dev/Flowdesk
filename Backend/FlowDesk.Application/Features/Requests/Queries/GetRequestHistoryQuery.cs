using FlowDesk.Application.Features.Requests.DTOs;
using MediatR;
using FlowDesk.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using FlowDesk.Domain.Enums;

namespace FlowDesk.Application.Features.Requests.Queries
{
    public record GetRequestHistoryQuery(int RequestId) : IRequest<List<RequestHistoryResponseDto>>;

    public class GetRequestHistoryQueryHandler(
        IRequestRepository requestRepository,
        IRequestHistoryRepository requestHistoryRepository,
        ILogger<GetRequestHistoryQueryHandler> logger)
        : IRequestHandler<GetRequestHistoryQuery, List<RequestHistoryResponseDto>>
    {
        public async Task<List<RequestHistoryResponseDto>> Handle(
            GetRequestHistoryQuery request,
            CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting {Operation} for RequestId {RequestId}",
                nameof(GetRequestHistoryQueryHandler), request.RequestId);

            var req = await requestRepository.GetByIdAsync(request.RequestId);
            if (req == null)
            {
                logger.LogWarning("Request not found with Id {RequestId}", request.RequestId);
                throw new Common.Exceptions.NotFoundException("Request not found.");
            }

            var history = await requestHistoryRepository.GetByRequestId(request.RequestId);

            var result = history?
                //.OrderByDescending(x => x.ChangedOn) // latest first
                .Select(x => new RequestHistoryResponseDto
                {
                    Action = x.OldStatus == null || x.OldStatus == RequestStatusEnum.Open ? "Created" : "StatusChanged",
                    OldStatus = x.OldStatus.HasValue ? (int)x.OldStatus.Value : (int?)null,
                    NewStatus = (int)x.NewStatus,
                    ChangedBy = x.IsSystemGenerated ? "System" : x.ChangedByUser?.FullName, // ideally map to name later
                    ChangedOn = x.ChangedOn,
                })
                .ToList() ?? new List<RequestHistoryResponseDto>();

            logger.LogInformation("Completed {Operation} for RequestId {RequestId}",
                nameof(GetRequestHistoryQueryHandler), request.RequestId);

            return result;
        }
    }
}

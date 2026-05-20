using FlowDesk.Application.Features.Requests.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Dashboard.Queries
{
    public record GetSlaSummaryQuery : IRequest<SlaStatusDto>;

    public class GetSlaSummaryQueryHandler(
        IRequestRepository requestRepository,
        ILogger<GetSlaSummaryQueryHandler> logger)
        : IRequestHandler<GetSlaSummaryQuery, SlaStatusDto>
    {
        public async Task<SlaStatusDto> Handle(GetSlaSummaryQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting {Operation}", nameof(GetSlaSummaryQueryHandler));

            var summary = await requestRepository.GetSlaSummaryAsync();

            logger.LogInformation("Completed {Operation}", nameof(GetSlaSummaryQueryHandler));

            return new SlaStatusDto
            {
                WithinSla = summary.WithinSla,
                NearingBreach = summary.NearingBreach,
                Breached = summary.Breached,
                Escalated = summary.Escalated
            };
        }
    }
}

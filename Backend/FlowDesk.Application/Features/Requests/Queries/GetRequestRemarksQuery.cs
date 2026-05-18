using FlowDesk.Application.Features.Requests.DTOs;
using MediatR;
using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Interfaces;

namespace FlowDesk.Application.Features.Requests.Queries
{
    public record GetRequestRemarksQuery(FilterRequestRemarksQueryDto Dto) : IRequest<List<RequestRemarkResponseDto>>;

    public class GetRequestRemarksQueryHandler(IRequestsService _requestsService) : IRequestHandler<GetRequestRemarksQuery, List<RequestRemarkResponseDto>>
    {
        public async Task<List<RequestRemarkResponseDto>> Handle(GetRequestRemarksQuery request, CancellationToken cancellationToken)
        {

            var remarksList = await _requestsService.GetRequestRemarksAsync(request.Dto);
            return remarksList.Select(r => new RequestRemarkResponseDto
            {
                MasterRemarksId = r.MasterRemarksId,
                RemarksText = r.RemarksText,
                ActionType = (int)r.ActionType
            }).ToList();
        }
    }
}

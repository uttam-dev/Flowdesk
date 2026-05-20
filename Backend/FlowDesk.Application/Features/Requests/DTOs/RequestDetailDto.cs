namespace FlowDesk.Application.Features.Requests.DTOs
{
    public class RequestDetailDto : RequestResponseDto
    {
        public List<EscalationHistoryDto>? EscalationHistory { get; set; }
    }
}

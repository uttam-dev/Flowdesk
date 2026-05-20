namespace FlowDesk.Application.Features.Requests.DTOs
{
    public class EscalationHistoryDto
    {
        public int EscalationId { get; set; }
        public string EscalatedByName { get; set; } = string.Empty;
        public DateTime EscalatedOn { get; set; }
        public string EscalationReason { get; set; } = string.Empty;
    }
}

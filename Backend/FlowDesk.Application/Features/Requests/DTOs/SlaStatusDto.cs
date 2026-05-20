namespace FlowDesk.Application.Features.Requests.DTOs
{
    public class SlaStatusDto
    {
        public int WithinSla { get; set; }
        public int NearingBreach { get; set; }
        public int Breached { get; set; }
        public int Escalated { get; set; }
    }
}

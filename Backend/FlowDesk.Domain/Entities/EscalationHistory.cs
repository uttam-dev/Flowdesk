namespace FlowDesk.Domain.Entities
{
    public class EscalationHistory
    {
        public int EscalationId { get; set; }
        public int RequestId { get; set; }
        public Request? Request { get; set; }

        public int EscalatedBy { get; set; }
        public User? EscalatedByUser { get; set; }

        public DateTime EscalatedOn { get; set; }
        public string EscalationReason { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
    }
}


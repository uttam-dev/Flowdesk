using FlowDesk.Domain.Enums;

namespace FlowDesk.Domain.Entities
{
    public class RequestHistory
    {
        public int RequestHistoryId { get; set; }

        public int RequestId { get; set; }
        public Request? Request { get; set; }

        public RequestStatusEnum OldStatus { get; set; }
        public RequestStatusEnum NewStatus { get; set; }

        public int ChangedById { get; set; }
        public User? ChangedByUser { get; set; }

        public DateTime ChangedOn { get; set; }
        public string? Remarks { get; set; }
    }
}

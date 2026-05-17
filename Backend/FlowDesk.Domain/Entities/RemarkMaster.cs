using FlowDesk.Domain.Enums;

namespace FlowDesk.Domain.Entities
{
    public class MasterRemarks
    {
        public int MasterRemarksId { get; set; }
        public string? RemarksText { get; set; }
        public RemarksActionTypeEnum ActionType { get; set; } // 1: Approve, 2: Reject, 3: Assign :4 Request Changes
        public bool IsActive { get; set; } = true;

        public ICollection<RequestHistory>? RequestHistories { get; set; }
    }
}

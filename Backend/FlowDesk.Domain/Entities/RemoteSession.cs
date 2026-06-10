using FlowDesk.Domain.Enums;

namespace FlowDesk.Domain.Entities
{
    public class RemoteSession
    {
        public int RemoteSessionId { get; set; }
        public int RequestId { get; set; }
        public Request Request { get; set; } = null!;

        public int InitiatedByUserId { get; set; }
        public User InitiatedBy { get; set; } = null!;

        public int TargetUserId { get; set; }
        public User TargetUser { get; set; } = null!;

        public RemoteSessionStatusEnum Status { get; set; } = RemoteSessionStatusEnum.Pending;
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public int? DurationSeconds { get; set; }
        public string? ResolutionNotes { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }

};
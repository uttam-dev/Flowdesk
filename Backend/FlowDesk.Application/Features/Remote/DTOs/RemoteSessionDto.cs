using FlowDesk.Domain.Enums;

namespace FlowDesk.Application.Features.Remote.DTOs
{
    public class RemoteSessionDto
    {
        public int Id { get; set; }
        public int RequestId { get; set; }

        public int InitiatedByUserId { get; set; }
        public string InitiatedByName { get; set; } = string.Empty;

        public int TargetUserId { get; set; }
        public string TargetUserName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public int? DurationSeconds { get; set; }

        public string? ResolutionNotes { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedAt { get; set; }
    }

};

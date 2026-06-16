using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Remote.DTOs
{
    public class RemoteSessionSummaryDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string InitiatedByName { get; set; } = string.Empty;
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public int? DurationSeconds { get; set; }
    }
}

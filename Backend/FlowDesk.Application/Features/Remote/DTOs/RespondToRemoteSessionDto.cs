using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Remote.DTOs
{
    public class RespondToRemoteSessionDto
    {
        public bool Accepted { get; set; }
        public string? RejectionReason { get; set; }
    }
}

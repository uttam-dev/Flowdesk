using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Dashboard.DTOs
{
    public class DashboardResponseDto
    {
        public int TotalRequests { get; set; }
        public int Open { get; set; }
        public int PendingApproval { get; set; }
        public int Assigned { get; set; }
        public int InProgress { get; set; }
        public int Resolved { get; set; }
        public int Closed { get; set; }
        public Dictionary<string, int> StatusChart { get; set; } = new();
        public string RoleName { get; set; } = string.Empty;
    }
}

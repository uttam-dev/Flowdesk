using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Requests.DTOs
{
    public class RequestResponseDto
    {
        public int RequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? CategoryName { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PriorityEnum Priority { get; set; }
        public RequestStatusEnum Status { get; set; }
        public string? AssignedUser { get; set; }
        public string? ApprovalName { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime? ClosedOn { get; set; }
        public DateTime? DueDate { get; set; }
        public string? SlaStatus { get; set; }
        public bool? IsEscalated { get; set; }
        public DateTime? EscalatedOn { get; set; }
        public string? EscalationReason { get; set; }
        public string? EscalatedByName { get; set; }
    }
}

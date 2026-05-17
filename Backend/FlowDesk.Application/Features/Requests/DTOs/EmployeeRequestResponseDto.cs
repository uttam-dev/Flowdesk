using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Requests.DTOs
{
    public class EmployeeRequestResponseDto
    {
        public string RequestNumber { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public PriorityEnum Priority { get; set; }
        public RequestStatusEnum Status { get; set; }
        public string? ApprovalName {get;set;}
        public string? AssignedToName { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ClosedOn { get; set; }
    }
}

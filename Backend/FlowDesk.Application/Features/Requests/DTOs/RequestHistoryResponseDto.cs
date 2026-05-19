using FlowDesk.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Requests.DTOs
{
    public class RequestHistoryResponseDto
    {
        public string? Action { get; set; }
        public int? OldStatus { get; set; }
        public int? NewStatus { get; set; }
        public string? ChangedBy { get; set; }
        public DateTime ChangedOn { get; set; }
    }
}

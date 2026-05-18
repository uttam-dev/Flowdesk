using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Requests.DTOs
{
    public class RequestRemarkResponseDto
    {
        public int MasterRemarksId { get; set; }
        public string? RemarksText { get; set; }
        public int ActionType { get; set; }
    }
}
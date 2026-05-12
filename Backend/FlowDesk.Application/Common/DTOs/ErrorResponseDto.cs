using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Common.DTOs
{
    public class ErrorResponseDto
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public List<string>? Errors { get; set; }
        public string TraceId { get; set; } = string.Empty;
    }
}

namespace FlowDesk.Domain.DTOs
{
    public class ErrorResponseDto
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public object? Details { get; set; }
        public List<string>? Errors { get; set; }
        public string TraceId { get; set; } = string.Empty;
    }
}

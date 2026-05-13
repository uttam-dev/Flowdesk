namespace FlowDesk.Domain.DTOs
{
    public class ApiResponseDto
    {
        public int StatusCode { get; set; } = 200;
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; } = null;
    }
}

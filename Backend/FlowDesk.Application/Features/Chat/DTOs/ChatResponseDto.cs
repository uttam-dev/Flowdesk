namespace FlowDesk.Application.Features.Chat.DTOs
{
    public class ChatResponseDto
    {
        public string Response { get; set; } = string.Empty;
        public bool IsCommandHandled { get; set; }
    }
}

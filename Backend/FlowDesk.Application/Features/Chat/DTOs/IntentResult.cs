using FlowDesk.Application.Features.Chat.Enums;

namespace FlowDesk.Application.Features.Chat.DTOs
{
    public class IntentResult
    {
        public IntentType Intent { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new();
        public bool IsResolved { get; set; }
    }
}

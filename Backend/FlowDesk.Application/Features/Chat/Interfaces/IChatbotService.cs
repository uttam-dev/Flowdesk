namespace FlowDesk.Application.Features.Chat.Interfaces
{
    public interface IChatbotService
    {
        Task<string> GetResponseAsync(string message, string systemPrompt, string contextData, CancellationToken cancellationToken = default);
    }
}

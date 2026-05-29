using FlowDesk.Application.Features.Chat.DTOs;

namespace FlowDesk.Application.Features.Chat.Interfaces
{
    public interface IEntityContextBuilder
    {
        Task<string> BuildContextAsync(int userId, string userRole, IntentResult intent, CancellationToken cancellationToken = default);
    }
}

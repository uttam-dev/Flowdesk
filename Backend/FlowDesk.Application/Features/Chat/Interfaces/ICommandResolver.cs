using FlowDesk.Application.Features.Chat.DTOs;

namespace FlowDesk.Application.Features.Chat.Interfaces
{
    public interface ICommandResolver
    {
        Task<IntentResult> ResolveAsync(string message, int userId, string userRole, CancellationToken cancellationToken = default);
        Task<string?> HandleIntentAsync(IntentResult intent, int userId, string userRole, CancellationToken cancellationToken = default);
    }
}

namespace FlowDesk.Application.Features.Chat.Interfaces
{
    public interface ICommandResolver
    {
        Task<string?> ResolveAsync(string message, int userId, string userRole, CancellationToken cancellationToken = default);
    }
}

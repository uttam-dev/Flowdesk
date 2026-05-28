namespace FlowDesk.Application.Features.Chat.Interfaces
{
    public interface IEntityContextBuilder
    {
        Task<string> BuildContextAsync(int userId, string userRole, CancellationToken cancellationToken = default);
    }
}

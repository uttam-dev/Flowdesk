using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;

namespace FlowDesk.Domain.Interfaces
{
    public interface IRemoteSessionRepository
    {
        Task<RemoteSession?> GetByIdAsync(int sessionId, CancellationToken ct = default);
        Task<RemoteSession?> GetLatestByRequestIdAsync(int requestId, CancellationToken ct = default);
        Task<IReadOnlyList<RemoteSession>> GetAllByRequestIdAsync(int requestId, CancellationToken ct = default);
        Task<IReadOnlyList<RemoteSession>> GetActiveSessionsAsync(CancellationToken ct = default);
        Task<bool> HasActiveSessionAsync(int requestId, CancellationToken ct = default);
        Task<RemoteSession> AddAsync(RemoteSession session, CancellationToken ct = default);
        Task UpdateAsync(RemoteSession session, CancellationToken ct = default);
    }

};
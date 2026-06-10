using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Interfaces;
using FlowDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.Infrastructure.Repositories
{
    public class RemoteSessionRepository : IRemoteSessionRepository
    {
        private readonly AppDbContext _context;

        public RemoteSessionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RemoteSession?> GetByIdAsync(int sessionId, CancellationToken ct = default)
        {
            return await _context.RemoteSessions
                .Include(x => x.InitiatedBy)
                .Include(x => x.TargetUser)
                .Include(x => x.Request)
                .FirstOrDefaultAsync(x => x.RemoteSessionId == sessionId, ct);
        }

        public async Task<RemoteSession?> GetLatestByRequestIdAsync(int requestId, CancellationToken ct = default)
        {
            return await _context.RemoteSessions
                .Include(x => x.InitiatedBy)
                .Include(x => x.TargetUser)
                .Where(x => x.RequestId == requestId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyList<RemoteSession>> GetAllByRequestIdAsync(int requestId, CancellationToken ct = default)
        {
            return await _context.RemoteSessions
                .Include(x => x.InitiatedBy)
                .Include(x => x.TargetUser)
                .Where(x => x.RequestId == requestId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<RemoteSession>> GetActiveSessionsAsync(CancellationToken ct = default)
        {
            return await _context.RemoteSessions
                .Include(x => x.InitiatedBy)
                .Include(x => x.TargetUser)
                .Include(x => x.Request)
                .Where(x => x.Status == RemoteSessionStatusEnum.Active)
                .OrderByDescending(x => x.StartedAt)
                .ToListAsync(ct);
        }

        public async Task<bool> HasActiveSessionAsync(int requestId, CancellationToken ct = default)
        {
            return await _context.RemoteSessions
                .AnyAsync(x =>
                    x.RequestId == requestId &&
                    (x.Status == RemoteSessionStatusEnum.Pending ||
                     x.Status == RemoteSessionStatusEnum.Accepted ||
                     x.Status == RemoteSessionStatusEnum.Active),
                    ct);
        }

        public async Task AddAsync(RemoteSession session, CancellationToken ct = default)
        {
            await _context.RemoteSessions.AddAsync(session, ct);
        }

        public Task UpdateAsync(RemoteSession session, CancellationToken ct = default)
        {
            session.UpdatedAt = DateTime.UtcNow;
            _context.RemoteSessions.Update(session);
            return Task.CompletedTask;
        }
    }

};
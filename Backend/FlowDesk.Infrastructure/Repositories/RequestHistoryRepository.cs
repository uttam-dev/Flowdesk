using FlowDesk.Infrastructure.Data;
using FlowDesk.Domain.Interfaces;
using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.Infrastructure.Repositories
{
    public class RequestHistoryRepository(AppDbContext _context) : IRequestHistoryRepository
    {
        public async Task AddAsync(RequestHistory requestHistory)
        {
            await _context.RequestHistories.AddAsync(requestHistory);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int requestHistoryId)
        {
            return await _context.RequestHistories.AnyAsync(rh => rh.RequestHistoryId == requestHistoryId);
        }

        public async Task<IReadOnlyList<RequestHistory>> GetAllAsync()
        {
            return await _context.RequestHistories.AsNoTracking().ToListAsync();
        }

        public async Task<RequestHistory?> GetByIdAsync(int requestHistoryId)
        {
            return await _context.RequestHistories.FirstOrDefaultAsync(rh => rh.RequestHistoryId == requestHistoryId);
        }

        public async Task<List<RequestHistory>?> GetByRequestId(int requestId)
        {
            return await _context.RequestHistories.Include(r => r.ChangedByUser)
                .ThenInclude(u => u!.Role)
                .Where(r => r.RequestId == requestId)
                .OrderBy(r => r.ChangedOn).ToListAsync();
        }

        public async Task HardDelete(RequestHistory requestHistory)
        {
            _context.RequestHistories.Remove(requestHistory);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task Update(RequestHistory requestHistory)
        {
            _context.RequestHistories.Update(requestHistory);
            await _context.SaveChangesAsync();
        }
    }
}

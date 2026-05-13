using FlowDesk.Infrastructure.Data;
using FlowDesk.Domain.Interfaces;
using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.Infrastructure.Repositories
{
    public class RequestRepository(AppDbContext _context) : IRequestRepository
    {
        public async Task AddAsync(Request request)
        {
            await _context.Requests.AddAsync(request);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int requestId)
        {
            return await _context.Requests.AnyAsync(r => r.RequestId == requestId);

        }

        public async Task<IReadOnlyList<Request>> GetAllAsync()
        {
            return await _context.Requests.AsNoTracking().ToListAsync();
        }

        public Task<Request?> GetByIdAsync(int requestId)
        {
            return _context.Requests.FirstOrDefaultAsync(r => r.RequestId == requestId);
        }

        public async Task HardDelete(Request request)
        {
            _context.Requests.Remove(request);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task Update(Request request)
        {
            _context.Requests.Update(request);
            await _context.SaveChangesAsync();
        }
    }
}

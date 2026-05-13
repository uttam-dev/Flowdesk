using FlowDesk.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Domain.Interfaces
{
    public interface IRequestRepository
    {
        Task<Request?> GetByIdAsync(int requestId);
        Task<IReadOnlyList<Request>> GetAllAsync();
        Task AddAsync(Request request);
        Task Update(Request request);
        Task HardDelete(Request request);
        Task<bool> ExistsAsync(int requestId);
        Task SaveChangesAsync();

    }
}

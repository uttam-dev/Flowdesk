using FlowDesk.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Domain.Interfaces
{
    public interface IRequestHistoryRepository
    {
            Task<RequestHistory?> GetByIdAsync(int requestHistoryId);
            Task<IReadOnlyList<RequestHistory>> GetAllAsync();
            Task AddAsync(RequestHistory requestHistory);
            Task Update(RequestHistory requestHistory);
            Task HardDelete(RequestHistory requestHistory);
            Task<bool> ExistsAsync(int requestHistoryId);
            Task SaveChangesAsync();
    }
}

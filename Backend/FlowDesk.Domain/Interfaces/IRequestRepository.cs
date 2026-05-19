using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Domain.Interfaces
{
    public interface IRequestRepository
    {
        Task<Request?> GetByIdAsync(int requestId);
        Task<(int, IReadOnlyList<Request?>)> GetAllAsync(FilterRequestQueryDto filter, int userId, string role);
        Task<(int, IReadOnlyList<Request?>)> GetAllTeamRequestsAsync(FilterRequestQueryDto filter, int userId);
        Task<Request> AddAsync(Request request);
        Task<Request> Update(Request request);
        Task HardDelete(Request request);
        Task<bool> ExistsAsync(int requestId);
        Task SaveChangesAsync();
        Task<(int Total, int Open, int PendingApproval, int Assigned, int InProgress, int Resolved, int Closed)> GetDashboardDataAsync(int userId, RoleEnum role);
    }
}

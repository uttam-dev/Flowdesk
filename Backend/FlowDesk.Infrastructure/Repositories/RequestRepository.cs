using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Interfaces;
using FlowDesk.Domain.Utils;
using FlowDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.Infrastructure.Repositories
{
    public class RequestRepository(AppDbContext _context) : IRequestRepository
    {
        public async Task<Request> AddAsync(Request request)
        {
            await _context.Requests.AddAsync(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<bool> ExistsAsync(int requestId)
        {
            return await _context.Requests.AnyAsync(r => r.RequestId == requestId);

        }

        // Fix CS8602: Add null check for Employee before accessing Manager in the query includes
        public async Task<(int, IReadOnlyList<Request?>)> GetAllAsync(
                        FilterRequestQueryDto filter,
                        int userId,
                        string role)
        {
            var query = _context.Requests
                .AsNoTracking()
                .Include(r => r.Category)
                .Include(r => r.Employee)
                .ThenInclude(e => e!.Manager)
                //.Where(r => r.Category!.IsApprovalRequired == true)
                .Include(r => r.AssignedUser)
                .Include(r => r.EscalatedByUser)
                .AsQueryable();

            // ROLE BASED FILTER
            if (role == RoleEnum.Employee.ToString())
            {
                query = query.Where(r => r.EmployeeId == userId);
            }
            else if (role == RoleEnum.Manager.ToString())
            {
                var teamIds = _context.Users
                    .Where(u => u.ManagerId == userId)
                    .Select(u => u.UserId)
                    .ToList();

                query = query.Where(r => r.EmployeeId == userId || teamIds.Contains(r.EmployeeId));
            }
            else if (role == RoleEnum.Support.ToString())
            {
                query = query.Where(r => r.AssignedToId == userId);
            }
            // Admin no filter

            // Existing filters
            if (filter.Status != null)
            {
                query = query.Where(r => (int)r.Status == filter.Status);
            }

            if (filter.CategoryId != null)
            {
                query = query.Where(r => r.CategoryId == filter.CategoryId);
            }

            if (filter.Priority != null)
            {
                query = query.Where(r => (int)r.Priority == filter.Priority);
            }

            if (filter.RequestNumber != null)
            {
                query = query.Where(r => r.RequestNumber == filter.RequestNumber);
            }
            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.CreatedOn)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (totalCount, items);
        }
        public async Task<(int, IReadOnlyList<Request?>)> GetAllTeamRequestsAsync(
                        FilterRequestQueryDto filter,
                        int userId)
        {
            var query = _context.Requests
                .AsNoTracking()
                .Include(r => r.Category)
                .Include(r => r.Employee)
                .Include(r => r.AssignedUser)
                .Include(r => r.EscalatedByUser)
                .Where(r => r.Employee != null && r.Employee.ManagerId == userId && r.Category!.IsApprovalRequired)
                .AsQueryable();

            // Existing filters
            if (filter.Status != null)
            {
                query = query.Where(r => (int)r.Status == filter.Status);
            }

            if (filter.CategoryId != null)
            {
                query = query.Where(r => r.CategoryId == filter.CategoryId);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.CreatedOn)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (totalCount, items);
        }

        public Task<Request?> GetByIdAsync(int requestId)
        {
            return _context.Requests
                            .AsNoTracking()
                            .Include(r => r.Category)
                            .Include(r => r.Employee)
                                .ThenInclude(e => e!.Manager!) // Suppress nullable warning as EF handles null navigation
                            .Include(r => r.AssignedUser)
                            .Include(r => r.EscalatedByUser)
                            .FirstOrDefaultAsync(r => r.RequestId == requestId);
        }

        public Task<Request?> GetByIdWithEscalationAsync(int requestId)
        {
            return _context.Requests
                            .Include(r => r.Category)
                            .Include(r => r.Employee)
                                .ThenInclude(e => e!.Manager!)
                            .Include(r => r.AssignedUser)
                            .Include(r => r.Comments)
                            .Include(r => r.Histories)
                            .Include(r => r.EscalatedByUser)
                            .Include(r => r.EscalationHistory!)
                                .ThenInclude(e => e.EscalatedByUser)
                            .FirstOrDefaultAsync(r => r.RequestId == requestId);
        }

        public async Task<Request?> GetByRequestNumberAsync(string requestNumber)
        {
            return await _context.Requests
                .AsNoTracking()
                .Include(r => r.Category)
                .Include(r => r.Employee)
                .Include(r => r.AssignedUser)
                .FirstOrDefaultAsync(r =>
                    r.RequestNumber == requestNumber ||
                    r.RequestNumber.Contains(requestNumber.Replace("REQ-", "")));
        }

        public Task<List<Request>> GetBreachedRequestsAsync()
        {
            return _context.Requests
                .Where(r => r.DueDate < DateTime.UtcNow
                    && r.Status != RequestStatusEnum.Resolved
                    && r.Status != RequestStatusEnum.Closed)
                .ToListAsync();
        }

        public async Task AddEscalationHistoryAsync(EscalationHistory escalation)
        {
            await _context.EscalationHistories.AddAsync(escalation);
        }

        public async Task<(int Total, int Open, int PendingApproval, int Assigned, int InProgress, int Resolved, int Closed)>
       GetDashboardDataAsync(int userId, RoleEnum role)
        {
            var baseQuery = _context.Requests
                .AsNoTracking()
                .AsQueryable();

            IQueryable<Request> query;

            if (role == RoleEnum.Employee)
            {
                query = baseQuery.Where(r => r.EmployeeId == userId);
            }
            else if (role == RoleEnum.Manager)
            {
                // Get team employee ids
                var teamIds = await _context.Users
                    .Where(u => u.ManagerId == userId)
                    .Select(u => u.UserId)
                    .ToListAsync();

                // Manager own requests + team pending approval
                query = baseQuery.Where(r =>
                    r.EmployeeId == userId // own requests
                    || (teamIds.Contains(r.EmployeeId)
                        && r.Status == RequestStatusEnum.PendingApproval) // team pending
                );
            }
            else if (role == RoleEnum.Support)
            {
                query = baseQuery.Where(r => r.AssignedToId == userId);
            }
            else
            {
                query = baseQuery;
            }

            var result = await query
                .GroupBy(x => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Open = g.Count(x => x.Status == RequestStatusEnum.Open),
                    PendingApproval = g.Count(x => x.Status == RequestStatusEnum.PendingApproval),
                    Assigned = g.Count(x => x.Status == RequestStatusEnum.Assigned),
                    InProgress = g.Count(x => x.Status == RequestStatusEnum.InProgress),
                    Resolved = g.Count(x => x.Status == RequestStatusEnum.Resolved),
                    Closed = g.Count(x => x.Status == RequestStatusEnum.Closed)
                })
                .OrderBy(x => x.Total)
                .FirstOrDefaultAsync();

            return result == null
                ? (0, 0, 0, 0, 0, 0, 0)
                : (result.Total, result.Open, result.PendingApproval,
                   result.Assigned, result.InProgress, result.Resolved, result.Closed);
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

        public async Task<Request> Update(Request request)
        {
            _context.Requests.Update(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<(int WithinSla, int NearingBreach, int Breached, int Escalated)> GetSlaSummaryAsync()
        {
            var now = DateTime.UtcNow;
            var activeRequests = _context.Requests.AsNoTracking()
                .Where(r => r.Status != RequestStatusEnum.Resolved && r.Status != RequestStatusEnum.Closed);

            var withinSla = await activeRequests
                .CountAsync(r => r.DueDate != null && now <= r.DueDate.Value.AddHours(-2));

            var nearingBreach = await activeRequests
                .CountAsync(r => r.DueDate != null && now > r.DueDate.Value.AddHours(-2) && now <= r.DueDate);

            var breached = await activeRequests
                .CountAsync(r => r.DueDate != null && now > r.DueDate);

            var escalated = await _context.Requests.AsNoTracking()
                .CountAsync(r => r.IsEscalated);

            return (withinSla, nearingBreach, breached, escalated);
        }
    }
}

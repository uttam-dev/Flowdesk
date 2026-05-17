using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Entities;
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
                    .ThenInclude(e => e!.Manager!)
                .Include(r => r.AssignedUser)
                .AsQueryable();

            // ROLE BASED FILTER
            if (role == RoleName.Employee)
            {
                query = query.Where(r => r.EmployeeId == userId);
            }
            else if (role == RoleName.Manager)
            {
                query = query.Where(r => r.EmployeeId == userId);
            }
            else if (role == RoleName.Support)
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
                            .FirstOrDefaultAsync(r => r.RequestId == requestId);
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
    }
}

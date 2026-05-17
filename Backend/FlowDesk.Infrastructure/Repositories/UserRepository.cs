using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Interfaces;
using FlowDesk.Domain.Utils;
using FlowDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace FlowDesk.Infrastructure.Repositories
{
    public class UserRepository(AppDbContext _context) : IUserRepository
    {
        public async Task<User> AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task Delete(User user)
        {
            user.IsDeleted = true;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email && !u.IsDeleted);
        }

        public async Task<bool> ExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.UserId == userId && !u.IsDeleted);
        }

        public async Task<IQueryable<User>> GetActivSeUsersAsync()
        {
            return _context.Users
                 .AsNoTracking()
                .Where(u => u.IsActive && !u.IsDeleted);
        }

        public async Task<List<User>> GetManagersAsync()
        {
            return await _context.Users.Include(u => u.Role)
                .Where(u => u.Role.RoleName == RoleName.Manager && !u.IsDeleted)
                .ToListAsync();
        }

        public async Task<(int, IReadOnlyList<User>)> GetAllAsync(FilterUserDataQueryDto filter)
        {
            var query = _context.Users
                .Include(u => u.Role)
                .Include(u => u.Manager)
                .Where(u => !u.IsDeleted)
                .AsNoTracking()
                .AsQueryable();

            // Optional filters
            if (!string.IsNullOrWhiteSpace(filter.Role))
                query = query.Where(u => u.Role.RoleName.ToLower() == filter.Role.ToLower());

            if (filter.IsActive.HasValue)
                query = query.Where(u => u.IsActive == filter.IsActive.Value);

            var totalPages = query.Count();
            var users = await query
                .OrderBy(x => x.UserId)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (totalPages, users);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Manager)
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Manager)
                .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);
        }

        public async Task HardDelete(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<User> Update(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<Domain.DTOs.ManagerResponseDto?> GetManagerByEmployeeId(int id)
        {
            var manager = await _context.Users
                .Include(u => u.Manager)
                .Where(u => u.UserId == id)
                .Select(u => new Domain.DTOs.ManagerResponseDto
                {
                    ManagerId = u.Manager != null ? u.Manager.UserId : 0,
                    FullName = u.Manager != null ? u.Manager.FullName : null
                })
                .FirstOrDefaultAsync();

            return manager;
        }

    }
}

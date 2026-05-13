using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Interfaces;
using FlowDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace FlowDesk.Infrastructure.Repositories
{
    public class UserRepository(AppDbContext _context) : IUserRepository
    {
        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
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

        public async Task<IReadOnlyList<User>> GetActiveUsersAsync()
        {
           return await _context.Users
                .AsNoTracking()
                .Where(u => u.IsActive && !u.IsDeleted).ToListAsync();
        }

        public async Task<IReadOnlyList<User>> GetAllAsync()
        {
           return await _context.Users
                .AsNoTracking()
                .Where(u => !u.IsDeleted).ToListAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            return await _context.Users
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

        public async Task Update(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}

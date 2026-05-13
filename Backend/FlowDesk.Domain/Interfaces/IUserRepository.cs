using FlowDesk.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int userId);
        Task<User?> GetByEmailAsync(string email);
        Task<IReadOnlyList<User>> GetAllAsync();
        Task<IReadOnlyList<User>> GetActiveUsersAsync();
        Task AddAsync(User user);
        Task Update(User user);
        Task Delete(User user);
        Task HardDelete(User user);
        Task<bool> ExistsAsync(int userId);
        Task<bool> EmailExistsAsync(string email);
        Task SaveChangesAsync();
    }
}

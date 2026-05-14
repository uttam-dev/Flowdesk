using FlowDesk.Domain.DTOs;
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
        Task<IReadOnlyList<User>> GetAllAsync(FilterUserDataQueryDto queryDto);
        Task<IQueryable<User>> GetActiveUsersAsync();
        Task<User> AddAsync(User user);
        Task<User> Update(User user);
        Task Delete(User user);
        Task HardDelete(User user);
        Task<bool> ExistsAsync(int userId);
        Task<bool> EmailExistsAsync(string email);
        Task SaveChangesAsync();
    }
}

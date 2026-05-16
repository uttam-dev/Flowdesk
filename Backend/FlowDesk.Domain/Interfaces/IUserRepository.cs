using FlowDesk.Domain.Entities;
using FlowDesk.Domain.DTOs;

namespace FlowDesk.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int userId);
        Task<User?> GetByEmailAsync(string email);
        Task<List<User>> GetManagersAsync();
        Task<(int, IReadOnlyList<User>)> GetAllAsync(FilterUserDataQueryDto queryDto);
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

using FlowDesk.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Domain.Interfaces
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(int categoryId);
        Task<IReadOnlyList<Category>> GetAllAsync();
        Task AddAsync(Category category);
        Task Update(Category category);
        Task ToggleActive(Category category);
        Task HardDelete(Category category);
        Task<bool> ExistsAsync(int categoryId);
        Task SaveChangesAsync();
    }
}

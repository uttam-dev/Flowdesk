using FlowDesk.Domain.Entities;
using FlowDesk.Domain.DTOs;

namespace FlowDesk.Domain.Interfaces
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(int categoryId);
        Task<Category?> GetByNameAsync(string categoryName);
        Task<(int, IReadOnlyList<Category>)> GetAllAsync(FilterCategoryDataQueryDto query);
        Task<Category> AddAsync(Category category);
        Task<Category> Update(Category category);
        Task ToggleActive(Category category);
        Task HardDelete(Category category);
        Task<bool> ExistsAsync(int categoryId);
        Task SaveChangesAsync();
    }
}

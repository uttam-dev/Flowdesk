using FlowDesk.Infrastructure.Data;
using FlowDesk.Domain.Interfaces;
using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.Infrastructure.Repositories
{
    public class CategoryRepository(AppDbContext _context) : ICategoryRepository
    {
        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int categoryId)
        {
            return await _context.Categories.AnyAsync(c => c.CategoryId == categoryId);
        }

        public async Task<IReadOnlyList<Category>> GetAllAsync()
        {
            return await _context.Categories.AsNoTracking().ToListAsync();
        }

        public Task<Category?> GetByIdAsync(int categoryId)
        {
            return _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId);
        }

        public async Task HardDelete(Category category)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task ToggleActive(Category category)
        {
            category.IsActive = !category.IsActive;
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

        }
    }
}

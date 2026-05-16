using FlowDesk.Infrastructure.Data;
using FlowDesk.Domain.Interfaces;
using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using FlowDesk.Application.Features.Categories.DTOs;
using FlowDesk.Domain.DTOs;

namespace FlowDesk.Infrastructure.Repositories
{
    public class CategoryRepository(AppDbContext _context) : ICategoryRepository
    {
        public async Task<Category> AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> ExistsAsync(int categoryId)
        {
            return await _context.Categories.AnyAsync(c => c.CategoryId == categoryId);
        }

        public async Task<(int,IReadOnlyList<Category>)> GetAllAsync(FilterCategoryDataQueryDto filter)
        {
            var query = _context.Categories
                .AsNoTracking()
                .AsQueryable();

            if (filter.IsActive.HasValue)
                query = query.Where(c => c.IsActive == filter.IsActive.Value);

            var totalPages = query.Count();

            var categories = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (totalPages,categories);
        }

        public async Task<Category?> GetByIdAsync(int categoryId)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId);
        }
        public async Task<Category?> GetByNameAsync(string categoryName)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.CategoryName == categoryName);
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

        public async Task<Category> Update(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }
    }
}

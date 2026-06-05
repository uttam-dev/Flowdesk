using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Interfaces;
using FlowDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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

        public async Task<(int, IReadOnlyList<Domain.Entities.Category>)> GetAllAsync(FilterCategoryDataQueryDto filter)
        {
            var query = _context.Categories
                .AsNoTracking()
                .AsQueryable();

            if (filter.RoleId.HasValue && filter.RoleId != (int)RoleEnum.Admin)
            {
                query = query.Where(c => c.IsActive);
                var total = query.Count();

                var cates = await query
                    .OrderBy(x => x.CategoryId)
                    .ToListAsync();

                return (total, cates);
            }
            else if (filter.IsActive.HasValue)
                query = query.Where(c => c.IsActive == filter.IsActive.Value);

            var totalPages = query.Count();

            var categories = await query
                .OrderBy(x => x.CategoryId)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (totalPages, categories);
        }

        public async Task<List<Domain.Entities.Category>> GetAllActiveAsync()
        {
            return await _context.Categories.Where(c => c.IsActive).ToListAsync();
        }
        public async Task<Domain.Entities.Category?> GetByIdAsync(int categoryId)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId);
        }
        public async Task<Domain.Entities.Category?> GetByNameAsync(string categoryName)
        {
            var normalizedRequestedName = categoryName.Replace(" ", "").ToLower();
            return await _context.Categories
                .FirstOrDefaultAsync(x =>
                    x.CategoryName.Replace(" ", "").ToLower() == normalizedRequestedName);
        }

        public async Task HardDelete(Domain.Entities.Category category)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task ToggleActive(Domain.Entities.Category category)
        {
            category.IsActive = !category.IsActive;
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task<Domain.Entities.Category> Update(Domain.Entities.Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }
    }
}

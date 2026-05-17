using FlowDesk.Infrastructure.Data;
using FlowDesk.Domain.Interfaces;
using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.Infrastructure.Repositories
{
    public class CommentRepository(AppDbContext _context) : ICommentRepository
    {
        public async Task<Comment> AddAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public Task<bool> ExistsAsync(int commentId)
        {
            return _context.Comments.AnyAsync(c => c.CommentId == commentId);
        }

        public async Task<IReadOnlyList<Comment>> GetAllAsync()
        {
            return await _context.Comments.AsNoTracking().ToListAsync();
        }

        public Task<Comment?> GetByIdAsync(int commentId)
        {
            return _context.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);
        }

        public async Task<IReadOnlyList<Comment>> GetByRequestIdAsync(int requestId)
        {
            return await _context.Comments
                .Include(c => c.Commenter)
                    .ThenInclude(u => u.Role)
                .Where(c => c.RequestId == requestId)
                .OrderBy(c => c.CreatedOn)
                .ToListAsync();
        }

        public async Task HardDelete(Comment comment)
        {
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task Update(Comment comment)
        {
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
        }
    }
}

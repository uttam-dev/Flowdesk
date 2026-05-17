using FlowDesk.Domain.Entities;

namespace FlowDesk.Domain.Interfaces
{
    public interface ICommentRepository
    {
            Task<Comment?> GetByIdAsync(int commentId);
            Task<IReadOnlyList<Comment>> GetAllAsync();
            Task<IReadOnlyList<Comment>> GetByRequestIdAsync(int requestId);
            Task<Comment> AddAsync(Comment comment);
            Task Update(Comment comment);
            Task HardDelete(Comment comment);
            Task<bool> ExistsAsync(int commentId);
            Task SaveChangesAsync();
    }
}

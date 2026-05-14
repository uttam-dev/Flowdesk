using FlowDesk.Domain.Entities;

namespace FlowDesk.Domain.Interfaces
{
    public interface IRefreshTokenService
    {
        Task SaveRefreshToken(int userId, string token);
        Task<RefreshToken?> GetToken(string token);
    }
}

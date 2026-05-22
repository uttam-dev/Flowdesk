using FlowDesk.Domain.Entities;
using FlowDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FlowDesk.Domain.Interfaces;

namespace FlowDesk.Infrastructure.Services
{
    public class RefreshTokenService(AppDbContext _context, IConfiguration configuration) : IRefreshTokenService
    {
        public async Task SaveRefreshToken(int userId, string token)
        {
            var refreshToken = new RefreshToken
            {
                Token = token,
                UserId = userId,
                ExpiryDate = DateTime.UtcNow.AddDays(Convert.ToInt32(configuration["RefreshToken:ExpireDays"]))
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetToken(string token)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsRevoked);
        }

        public async Task<RefreshToken?> Update(RefreshToken token)
        {
            _context.RefreshTokens.Update(token);
            await _context.SaveChangesAsync();
            return token;
        }
    }
}

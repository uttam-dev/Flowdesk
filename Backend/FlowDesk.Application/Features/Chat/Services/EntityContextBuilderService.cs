using System.Text.Json;
using FlowDesk.Application.Features.Chat.Interfaces;
using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Chat.Services
{
    public class EntityContextBuilderService(
        IUserRepository userRepository,
        IRequestRepository requestRepository,
        ILogger<EntityContextBuilderService> logger) : IEntityContextBuilder
    {
        public async Task<string> BuildContextAsync(int userId, string userRole, CancellationToken cancellationToken)
        {
            try
            {
                var context = new Dictionary<string, object>
                {
                    ["user"] = await GetUserContext(userId, cancellationToken),
                    ["requests_summary"] = await GetRequestsSummary(userId, userRole, cancellationToken)
                };

                return JsonSerializer.Serialize(context, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to build full context for UserId {UserId}. Returning partial context.", userId);
                return JsonSerializer.Serialize(new { error = "Context unavailable", user_id = userId, role = userRole });
            }
        }

        private async Task<object> GetUserContext(int userId, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
                return new { id = userId, error = "User not found" };

            return new
            {
                id = user.UserId,
                name = user.FullName,
                email = user.Email,
                role = user.Role.RoleName,
                roleId = user.RoleId,
                isActive = user.IsActive,
                createdOn = user.CreatedOn.ToString("yyyy-MM-dd")
            };
        }

        private async Task<object> GetRequestsSummary(int userId, string userRole, CancellationToken cancellationToken)
        {
            try
            {
                var filter = new FilterRequestQueryDto { PageNumber = 1, PageSize = 100 };
                var (totalCount, requests) = await requestRepository.GetAllAsync(filter, userId, userRole);

                var statusGroups = requests
                    .Where(r => r != null)
                    .GroupBy(r => r!.Status.ToString())
                    .ToDictionary(g => g.Key, g => g.Count());

                var recentRequests = requests
                    .Where(r => r != null)
                    .OrderByDescending(r => r!.CreatedOn)
                    .Take(5)
                    .Select(r => new
                    {
                        id = r!.RequestId,
                        number = r.RequestNumber,
                        title = r.Title.Length > 50 ? r.Title[..50] + "..." : r.Title,
                        status = r.Status.ToString(),
                        createdOn = r.CreatedOn.ToString("yyyy-MM-dd")
                    })
                    .ToList();

                return new
                {
                    totalCount,
                    statusBreakdown = statusGroups,
                    recentRequests
                };
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to fetch request summary for UserId {UserId}", userId);
                return new { totalCount = 0, error = "Could not load requests" };
            }
        }
    }
}

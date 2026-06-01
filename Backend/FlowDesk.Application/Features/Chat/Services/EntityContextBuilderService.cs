using System.Text.Json;
using FlowDesk.Application.Features.Chat.DTOs;
using FlowDesk.Application.Features.Chat.Enums;
using FlowDesk.Application.Features.Chat.Interfaces;
using FlowDesk.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Chat.Services
{
    public class EntityContextBuilderService(
        IUserRepository userRepository,
        IRequestRepository requestRepository,
        ICategoryRepository categoryRepository,
        ICacheService cache,
        ILogger<EntityContextBuilderService> logger) : IEntityContextBuilder
    {
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public async Task<string> BuildContextAsync(int userId, string userRole, IntentResult intent, CancellationToken ct)
        {
            try
            {
                var ctx = new Dictionary<string, object>();

                var user = await cache.GetOrCreateAsync(
                    $"chat_user_{userId}",
                    () => FetchUserContext(userId, ct),
                    TimeSpan.FromMinutes(5));
                ctx["u"] = user ?? new { id = userId, rl = userRole } as object;

                switch (intent.Intent)
                {
                    case IntentType.RequestDetail:
                        if (intent.Parameters.TryGetValue("reqNumber", out var num))
                        {
                            var detail = await FetchRequestDetail(num, userId, userRole, ct);
                            if (detail != null) ctx["req"] = detail;
                        }
                        break;

                    case IntentType.SlaStatus:
                        var sla = await cache.GetOrCreateAsync(
                            $"chat_sla_{userId}_{userRole}",
                            () => FetchSlaContext(userId, userRole, ct),
                            TimeSpan.FromSeconds(30));
                        ctx["sla"] = sla!;
                        break;

                    case IntentType.EscalatedRequests:
                        var esc = await cache.GetOrCreateAsync(
                            $"chat_esc_{userId}_{userRole}",
                            () => FetchEscalatedContext(userId, userRole, ct),
                            TimeSpan.FromSeconds(30));
                        ctx["escalated"] = esc!;
                        break;

                    case IntentType.CategoryInfo:
                        var cats = await cache.GetOrCreateAsync(
                            "chat_categories",
                            () => FetchCategoryContext(ct),
                            TimeSpan.FromMinutes(10));
                        ctx["categories"] = cats!;
                        break;

                    case IntentType.RequestSummary:
                        var sum = await cache.GetOrCreateAsync(
                            $"chat_sum_{userId}_{userRole}",
                            () => FetchSummaryContext(userId, userRole, ct),
                            TimeSpan.FromSeconds(30));
                        ctx["summary"] = sum!;
                        break;

                    case IntentType.TeamSummary when userRole == "Manager":
                        var team = await cache.GetOrCreateAsync(
                            $"chat_team_{userId}",
                            () => FetchLightweightSummary(userId, userRole, ct),
                            TimeSpan.FromSeconds(30));
                        ctx["r"] = team!;
                        break;

                    case IntentType.GeneralQuery:
                    default:
                        var general = await cache.GetOrCreateAsync(
                            $"chat_summary_{userId}_{userRole}",
                            () => FetchLightweightSummary(userId, userRole, ct),
                            TimeSpan.FromSeconds(30));
                        ctx["r"] = general!;
                        break;
                }

                return JsonSerializer.Serialize(ctx, JsonOpts);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Context build failed for {UserId}", userId);
                return "{\"u\":{\"id\":" + userId + ",\"rl\":\"" + userRole + "\"}}";
            }
        }

        private async Task<object?> FetchUserContext(int userId, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            return new
            {
                id = userId,
                rl = user.Role.RoleName,
                nm = user.FullName
            };
        }

        private async Task<object> FetchLightweightSummary(int userId, string userRole, CancellationToken cancellationToken)
        {
            try
            {
                var filter = new Domain.DTOs.FilterRequestQueryDto { PageNumber = 1, PageSize = 5 };
                var (totalCount, requests) = await requestRepository.GetAllAsync(filter, userId, userRole);

                var recent = requests
                    .Where(r => r != null)
                    .Select(r => new
                    {
                        id = r!.RequestId,
                        n = r.RequestNumber,
                        s = r.Status.ToString(),
                        p = r.Priority.ToString(),
                        t = r.Title.Length > 30 ? r.Title[..30] + ".." : r.Title
                    })
                    .ToList();

                return new { t = totalCount, l = recent };
            }
            catch
            {
                return new { t = 0, l = Array.Empty<object>() };
            }
        }

        private async Task<object?> FetchRequestDetail(string reqNumber, int userId, string userRole, CancellationToken ct)
        {
            var request = await requestRepository.GetByRequestNumberAsync(reqNumber);

            if (request == null) return null;

            if (userRole == "Employee" && request.EmployeeId != userId)
                return new { error = "not_found" };

            if (userRole == "Support" && request.AssignedToId != userId)
                return new { error = "not_found" };

            return new
            {
                number = request.RequestNumber,
                title = request.Title,
                status = request.Status.ToString(),
                priority = request.Priority.ToString(),
                category = request.Category?.CategoryName,
                raisedBy = request.Employee?.FullName,
                assignedTo = request.AssignedUser?.FullName ?? "Not assigned",
                dueDate = request.DueDate?.ToString("dd MMM yyyy HH:mm") ?? "N/A",
                escalated = request.IsEscalated,
                created = request.CreatedOn.ToString("dd MMM yyyy")
            };
        }

        private async Task<object> FetchSlaContext(int userId, string userRole, CancellationToken ct)
        {
            var filter = new Domain.DTOs.FilterRequestQueryDto { PageNumber = 1, PageSize = 10 };
            var (_, requests) = await requestRepository.GetAllAsync(filter, userId, userRole);

            var now = DateTime.UtcNow;
            var active = requests
                .Where(r => r != null && r.DueDate != null && (int)r.Status < 7)
                .Select(r => new
                {
                    number = r!.RequestNumber,
                    title = r.Title.Length > 30 ? r.Title[..30] + ".." : r.Title,
                    due = r.DueDate!.Value.ToString("dd MMM yyyy HH:mm"),
                    sla = r.DueDate < now ? "Breached" : r.DueDate < now.AddHours(2) ? "Nearing Breach" : "Within SLA",
                    escalated = r.IsEscalated
                })
                .ToList();

            return new
            {
                total = active.Count,
                breached = active.Count(x => x.sla == "Breached"),
                nearing = active.Count(x => x.sla == "Nearing Breach"),
                items = active.Take(8)
            };
        }

        private async Task<object> FetchEscalatedContext(int userId, string userRole, CancellationToken ct)
        {
            var filter = new Domain.DTOs.FilterRequestQueryDto { PageNumber = 1, PageSize = 10 };
            var (_, requests) = await requestRepository.GetAllAsync(filter, userId, userRole);

            var escalated = requests
                .Where(r => r != null && r.IsEscalated && (int)r.Status < 7)
                .Select(r => new
                {
                    number = r!.RequestNumber,
                    title = r.Title.Length > 30 ? r.Title[..30] + ".." : r.Title,
                    status = r.Status.ToString(),
                    priority = r.Priority.ToString(),
                    due = r.DueDate?.ToString("dd MMM yyyy") ?? "N/A"
                })
                .ToList();

            return new { count = escalated.Count, items = escalated };
        }

        private async Task<object> FetchCategoryContext(CancellationToken ct)
        {
            var categories = await categoryRepository.GetAllAsync(new Domain.DTOs.FilterCategoryDataQueryDto { PageNumber = 1, PageSize = 50 });

            var items = categories.Item2.Select(c => new
            {
                id = c.CategoryId,
                name = c.CategoryName,
                slaHours = c.SLAHours,
                requiresApproval = c.IsApprovalRequired,
                isActive = c.IsActive
            }).ToList();

            return new { total = categories.Item1, categories = items };
        }

        private async Task<object> FetchSummaryContext(int userId, string userRole, CancellationToken ct)
        {
            var filter = new Domain.DTOs.FilterRequestQueryDto { PageNumber = 1, PageSize = 50 };
            var (total, requests) = await requestRepository.GetAllAsync(filter, userId, userRole);

            var groups = requests
                .Where(r => r != null)
                .GroupBy(r => r!.Status.ToString())
                .Select(g => new { status = g.Key, count = g.Count() })
                .ToList();

            return new { total = total, byStatus = groups };
        }
    }
}

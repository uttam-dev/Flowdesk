using System.Text.RegularExpressions;
using FlowDesk.Application.Features.Chat.Interfaces;
using FlowDesk.Application.Features.Requests.Queries;
using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Chat.Services
{
    public class CommandResolverService(
        IMediator mediator,
        IUserRepository userRepository,
        IRequestRepository requestRepository,
        ILogger<CommandResolverService> logger) : ICommandResolver
    {
        public async Task<string?> ResolveAsync(string message, int userId, string userRole, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            try
            {
                if (IsMatch(message, ["my requests", "my open requests", "show my requests", "list my requests"]))
                    return await HandleMyRequests(userId, userRole, cancellationToken);

                if (IsMatch(message, ["status of request", "check request status", "request status"]))
                    return await HandleRequestStatus(message, cancellationToken);

                if (IsMatch(message, ["how to create", "create a request", "how do i create"]))
                    return GetCreateRequestHelp();

                if (IsMatch(message, ["what can i do", "help", "available commands"]))
                    return GetAvailableCommands(userRole);

                if (IsMatch(message, ["who am i", "my details", "my profile", "my info"]))
                    return await HandleUserProfile(userId, cancellationToken);

                if (IsMatch(message, ["pending approval", "pending requests", "waiting for approval"]))
                    return await HandlePendingApprovals(userId, userRole, cancellationToken);

                if (IsMatch(message, ["assigned to me", "my assignments", "my support requests"]))
                    return await HandleAssignedRequests(userId, userRole, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error resolving command for UserId {UserId}", userId);
                return null;
            }

            return null;
        }

        private static bool IsMatch(string input, string[] patterns)
        {
            return patterns.Any(p => input.Contains(p, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<string?> HandleMyRequests(int userId, string userRole, CancellationToken cancellationToken)
        {
            var filter = new FilterRequestQueryDto { PageNumber = 1, PageSize = 5 };
            var query = new GetAllRequestsQuery(filter, userId, userRole);
            var result = await mediator.Send(query, cancellationToken);

            if (result.Items.Count == 0)
                return "You have no requests at the moment.";

            var lines = result.Items.Select(r =>
                $"• **{r.RequestNumber}** - {r.Title} (_Status: {r.Status}_)");

            return $"Here are your recent requests:\n\n{string.Join("\n", lines)}";
        }

        private async Task<string?> HandleRequestStatus(string message, CancellationToken cancellationToken)
        {
            var match = Regex.Match(message, @"\d+");
            if (!match.Success)
                return "Please provide a request number. Example: 'status of request 123'.";

            if (!int.TryParse(match.Value, out var requestId))
                return "Invalid request ID.";

            var request = await requestRepository.GetByIdAsync(requestId);
            if (request == null)
                return $"Request #{requestId} not found.";

            return $"**Request {request.RequestNumber}**\n" +
                   $"- **Title**: {request.Title}\n" +
                   $"- **Status**: {request.Status}\n" +
                   $"- **Priority**: {request.Priority}\n" +
                   $"- **Created**: {request.CreatedOn:yyyy-MM-dd}";
        }

        private static string GetCreateRequestHelp()
        {
            return "To create a new request:\n\n" +
                   "1. Go to **Requests** in the sidebar.\n" +
                   "2. Click **Create Request**.\n" +
                   "3. Fill in the category, title, description, and priority.\n" +
                   "4. Submit — you will receive a request number.";
        }

        private static string GetAvailableCommands(string userRole)
        {
            var commands = new List<string>
            {
                "• \"my requests\" - View your requests",
                "• \"status of request {id}\" - Check request status",
                "• \"how to create a request\" - Get creation help",
                "• \"my profile\" - View your details",
                "• \"help\" - Show this list"
            };

            if (userRole is nameof(RoleEnum.Manager) or nameof(RoleEnum.Admin))
                commands.Add("• \"pending approvals\" - View requests awaiting approval");

            if (userRole is nameof(RoleEnum.Support) or nameof(RoleEnum.Admin))
                commands.Add("• \"assigned to me\" - View assigned requests");

            return $"I can help with the following:\n\n{string.Join("\n", commands)}";
        }

        private async Task<string?> HandleUserProfile(int userId, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
                return "User not found.";

            return $"**Your Profile**\n" +
                   $"- **Name**: {user.FullName}\n" +
                   $"- **Email**: {user.Email}\n" +
                   $"- **Role**: {user.Role.RoleName}";
        }

        private async Task<string?> HandlePendingApprovals(int userId, string userRole, CancellationToken cancellationToken)
        {
            if (userRole is not (nameof(RoleEnum.Manager) or nameof(RoleEnum.Admin)))
                return "You don't have permission to view pending approvals.";

            var filter = new FilterRequestQueryDto
            {
                PageNumber = 1,
                PageSize = 10,
                Status = (int)RequestStatusEnum.PendingApproval
            };
            var query = new GetAllRequestsQuery(filter, userId, userRole);
            var result = await mediator.Send(query, cancellationToken);

            if (result.Items.Count == 0)
                return "No requests pending approval.";

            var lines = result.Items.Select(r =>
                $"• **{r.RequestNumber}** - {r.Title} by {r.FullName}");

            return $"Requests pending approval:\n\n{string.Join("\n", lines)}";
        }

        private async Task<string?> HandleAssignedRequests(int userId, string userRole, CancellationToken cancellationToken)
        {
            if (userRole is not (nameof(RoleEnum.Support) or nameof(RoleEnum.Admin)))
                return "You don't have permission to view assigned requests.";

            var filter = new FilterRequestQueryDto { PageNumber = 1, PageSize = 10 };
            var query = new GetAllRequestsQuery(filter, userId, userRole);
            var result = await mediator.Send(query, cancellationToken);

            if (result.Items.Count == 0)
                return "You have no assigned requests.";

            var lines = result.Items.Select(r =>
                $"• **{r.RequestNumber}** - {r.Title} (_Status: {r.Status}_)");

            return $"Your assigned requests:\n\n{string.Join("\n", lines)}";
        }
    }
}

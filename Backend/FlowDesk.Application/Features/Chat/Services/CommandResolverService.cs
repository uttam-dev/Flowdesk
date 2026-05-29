using System.Text.RegularExpressions;
using FlowDesk.Application.Features.Chat.DTOs;
using FlowDesk.Application.Features.Chat.Enums;
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
        public Task<IntentResult> ResolveAsync(string message, int userId, string userRole, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(message))
                return Task.FromResult(new IntentResult { Intent = IntentType.GeneralQuery, IsResolved = false });

            var lower = message.ToLowerInvariant().Trim();

            if (MatchesAny(lower, ["who are you", "what are you", "your name", "introduce yourself", "are you ai", "are you a bot"]))
                return Resolved(IntentType.BotIdentity);

            if (MatchesAny(lower, ["help", "what can i do", "what can you do", "commands", "what do you know", "guide me"]))
                return Resolved(IntentType.Help);

            if (MatchesAny(lower, ["hi", "hello", "hey", "hyy", "hii", "heyy", "heyyy", "helo", "hiii", "good morning", "good afternoon", "good evening", "how are you", "howdy", "sup", "what's up", "greetings", "yo"]))
                return Resolved(IntentType.Greeting);

            if (MatchesAny(lower, ["how to approve", "how do i approve", "approve request", "can i approve", "who can approve", "approval process"]))
                return Resolved(IntentType.HowToApprove);

            if (MatchesAny(lower, ["how to assign", "how do i assign", "assign request", "who can assign", "can i assign"]))
                return Resolved(IntentType.HowToAssign);

            if (MatchesAny(lower, ["how to comment", "can i comment", "who can comment", "add comment", "comment on request", "comment condition", "conditions for comment"]))
                return Resolved(IntentType.HowToComment);

            if (MatchesAny(lower, ["how to escalate", "can i escalate", "who can escalate", "escalate request", "escalation process"]))
                return Resolved(IntentType.HowToEscalate);

            if (MatchesAny(lower, ["how to update status", "how to resolve", "mark resolved", "mark in progress", "update status", "who can update status", "can i resolve"]))
                return Resolved(IntentType.HowToUpdateStatus);

            if (MatchesAny(lower, ["how to create", "create a request", "new request", "raise request", "how do i create", "how to raise"]))
                return Resolved(IntentType.HowToCreateRequest);

            if (MatchesAny(lower, ["what is status", "explain status", "status meaning", "what does status mean", "status types", "request statuses"]))
                return Resolved(IntentType.ExplainStatus);

            if (MatchesAny(lower, ["what are roles", "explain roles", "role access", "who can what", "what can employee do", "what can manager do", "what can admin do", "what can support do", "role permissions"]))
                return Resolved(IntentType.ExplainRoles);

            if (MatchesAny(lower, ["role of admin", "role of manager", "role of employee", "role of support", "what is admin", "what is manager", "what does admin do", "what does manager do", "what does employee do", "what does support do", "admin responsibilities", "manager responsibilities"]))
                return Resolved(IntentType.ExplainRoles);

            if (lower.Contains("as a") || lower.Contains("as an") || lower.Contains("what should i do") || lower.Contains("what can i do as"))
                return Resolved(IntentType.Help);

            if (MatchesAny(lower, ["who am i", "my profile", "my details", "my info", "show my profile", "my role"]))
                return Resolved(IntentType.UserProfile);

            if (MatchesAny(lower, ["my requests", "show my requests", "list my requests", "all my requests", "my all requests", "show all", "recent requests", "all requests", "show recent", "list all requests", "view requests", "show requests", "list requests"]))
                return Resolved(IntentType.MyRequests);

            if (MatchesAny(lower, ["my open requests", "open requests", "show open", "list open"]))
                return Resolved(IntentType.OpenRequests);

            if (MatchesAny(lower, ["pending approval", "pending approvals", "awaiting approval", "waiting for approval", "needs approval"]))
                return Resolved(IntentType.PendingApprovals);

            if (MatchesAny(lower, ["assigned to me", "my assignments", "my assigned", "support requests", "what is assigned"]))
                return Resolved(IntentType.AssignedToMe);

            var reqMatch = Regex.Match(message, @"#?REQ-\d+-\d+", RegexOptions.IgnoreCase);
            if (reqMatch.Success)
                return Task.FromResult(new IntentResult
                {
                    Intent = IntentType.RequestDetail,
                    IsResolved = true,
                    Parameters = new Dictionary<string, string> { ["reqNumber"] = reqMatch.Value.ToUpper() }
                });

            if (MatchesAny(lower, ["sla", "due date", "deadline", "breach", "breached", "overdue", "nearing", "sla status", "due soon", "how many breached", "requests breached", "sla breach"]))
                return Task.FromResult(new IntentResult { Intent = IntentType.SlaStatus, IsResolved = false });

            if (MatchesAny(lower, ["escalated", "escalation", "escalated requests", "show escalated", "urgent requests"]))
                return Task.FromResult(new IntentResult { Intent = IntentType.EscalatedRequests, IsResolved = false });

            if (MatchesAny(lower, ["categor", "request type", "type of request", "what can i raise", "available categories", "what categories"]))
                return Task.FromResult(new IntentResult { Intent = IntentType.CategoryInfo, IsResolved = false });

            if (MatchesAny(lower, ["how many", "total", "count", "summary", "overview", "statistics", "stats", "dashboard"]))
                return Task.FromResult(new IntentResult { Intent = IntentType.RequestSummary, IsResolved = false });

            return Task.FromResult(new IntentResult { Intent = IntentType.GeneralQuery, IsResolved = false });
        }

        public async Task<string?> HandleIntentAsync(IntentResult intent, int userId, string userRole, CancellationToken cancellationToken)
        {
            try
            {
                return intent.Intent switch
                {
                    IntentType.MyRequests => await HandleMyRequests(userId, userRole, cancellationToken),
                    IntentType.OpenRequests => await HandleOpenRequests(userId, userRole, cancellationToken),
                    IntentType.RequestDetail => await HandleRequestDetail(intent, cancellationToken),
                    IntentType.PendingApprovals => await HandlePendingApprovals(userId, userRole, cancellationToken),
                    IntentType.AssignedToMe => await HandleAssignedToMe(userId, userRole, cancellationToken),
                    IntentType.UserProfile => await HandleUserProfile(userId, cancellationToken),
                    IntentType.Help => GetHelpMessage(userRole),
                    IntentType.CreateRequestHelp => GetCreateRequestHelp(),
                    IntentType.ExplainStatus => GetExplainStatusHelp(),
                    IntentType.Greeting => GetGreetingMessage(userRole),
                    IntentType.BotIdentity => "I am Flowdesk AI, your helpdesk assistant. I can help you with requests, statuses, SLA, and navigating the system.",
                    IntentType.HowToApprove => GetHowToApprove(userRole),
                    IntentType.HowToAssign => GetHowToAssign(userRole),
                    IntentType.HowToComment => GetHowToComment(userRole),
                    IntentType.HowToEscalate => GetHowToEscalate(userRole),
                    IntentType.HowToUpdateStatus => GetHowToUpdateStatus(userRole),
                    IntentType.HowToCreateRequest => GetCreateRequestHelp(),
                    IntentType.ExplainRoles => GetExplainRoles(),
                    _ => null
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error handling intent {Intent} for UserId {UserId}", intent.Intent, userId);
                return null;
            }
        }

        private static bool MatchesAny(string input, string[] patterns)
        {
            return patterns.Any(p => input.Contains(p, StringComparison.OrdinalIgnoreCase));
        }

        private static Task<IntentResult> Resolved(IntentType intent) =>
            Task.FromResult(new IntentResult { Intent = intent, IsResolved = true });

        private async Task<string?> HandleMyRequests(int userId, string userRole, CancellationToken cancellationToken)
        {
            var filter = new FilterRequestQueryDto { PageNumber = 1, PageSize = 5 };
            var query = new GetAllRequestsQuery(filter, userId, userRole);
            var result = await mediator.Send(query, cancellationToken);

            if (result.Items.Count == 0)
                return "You have no requests at the moment.";

            var lines = result.Items.Select((r, i) =>
                $"{i + 1}. Request {r.RequestNumber} - {Truncate(r.Title, 40)} - Status: {r.Status}");

            return $"Your recent requests:\n{string.Join("\n", lines)}";
        }

        private async Task<string?> HandleOpenRequests(int userId, string userRole, CancellationToken cancellationToken)
        {
            var filter = new FilterRequestQueryDto { PageNumber = 1, PageSize = 10, Status = (int)RequestStatusEnum.Open };
            var query = new GetAllRequestsQuery(filter, userId, userRole);
            var result = await mediator.Send(query, cancellationToken);

            if (result.Items.Count == 0)
                return "You have no open requests.";

            var lines = result.Items.Select((r, i) =>
                $"{i + 1}. Request {r.RequestNumber} - {Truncate(r.Title, 40)} - Priority: {r.Priority}");

            return $"Open requests:\n{string.Join("\n", lines)}";
        }

        private async Task<string?> HandleRequestDetail(IntentResult intent, CancellationToken cancellationToken)
        {
            if (!intent.Parameters.TryGetValue("reqNumber", out var reqNumber))
                return "Please provide a valid request number.";

            var cleanNumber = reqNumber.TrimStart('#').ToUpper();
            var request = await requestRepository.GetByRequestNumberAsync(cleanNumber);
            if (request == null)
                return "Request not found.";

            return $"Request {request.RequestNumber}\n" +
                   $"Title: {request.Title}\n" +
                   $"Status: {request.Status}\n" +
                   $"Priority: {request.Priority}\n" +
                   $"Created: {request.CreatedOn:yyyy-MM-dd}";
        }

        private async Task<string?> HandlePendingApprovals(int userId, string userRole, CancellationToken cancellationToken)
        {
            if (userRole is not (nameof(RoleEnum.Manager) or nameof(RoleEnum.Admin)))
                return "You don't have permission to view pending approvals.";

            var filter = new FilterRequestQueryDto { PageNumber = 1, PageSize = 10, Status = (int)RequestStatusEnum.PendingApproval };
            var query = new GetAllRequestsQuery(filter, userId, userRole);
            var result = await mediator.Send(query, cancellationToken);

            if (result.Items.Count == 0)
                return "No requests pending approval.";

            var lines = result.Items.Select((r, i) =>
                $"{i + 1}. Request {r.RequestNumber} - {Truncate(r.Title, 40)}");

            return $"Requests pending approval:\n{string.Join("\n", lines)}";
        }

        private async Task<string?> HandleAssignedToMe(int userId, string userRole, CancellationToken cancellationToken)
        {
            if (userRole is not (nameof(RoleEnum.Support) or nameof(RoleEnum.Admin)))
                return "You don't have permission to view assigned requests.";

            var filter = new FilterRequestQueryDto { PageNumber = 1, PageSize = 10 };
            var query = new GetAllRequestsQuery(filter, userId, userRole);
            var result = await mediator.Send(query, cancellationToken);

            if (result.Items.Count == 0)
                return "You have no assigned requests.";

            var lines = result.Items.Select((r, i) =>
                $"{i + 1}. Request {r.RequestNumber} - {Truncate(r.Title, 40)} - Status: {r.Status}");

            return $"Your assigned requests:\n{string.Join("\n", lines)}";
        }

        private async Task<string?> HandleUserProfile(int userId, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
                return "User not found.";

            return $"Name: {user.FullName}\nRole: {user.Role.RoleName}\nEmail: {user.Email}";
        }

        private static string GetHelpMessage(string userRole)
        {
            var commands = new List<string>
            {
                "- my requests       - View your requests",
                "- open requests     - View open requests",
                "- REQ-XXXXXX-XXXXXX - Check a specific request",
                "- my profile        - View your details",
                "- how to create     - How to raise a request",
                "- explain status    - What each status means",
                "- explain roles     - What each role can do",
                "- sla status        - Check SLA and due dates",
                "- escalated         - View escalated requests",
                "- categories        - Available request categories",
                "- summary           - Request count overview",
                "- how to approve    - Approve/reject guidance",
                "- how to assign     - Assignment guidance",
                "- how to comment    - Comment guidance",
                "- how to escalate   - Escalation guidance",
                "- how to resolve    - Status update guidance",
                "- help              - Show this list"
            };

            if (userRole is nameof(RoleEnum.Manager) or nameof(RoleEnum.Admin))
                commands.Add("- pending approvals - Requests waiting for approval");

            if (userRole is nameof(RoleEnum.Support) or nameof(RoleEnum.Admin))
                commands.Add("- assigned to me    - Requests assigned to you");

            return $"Here is what I can help with:\n{string.Join("\n", commands)}";
        }

        private static string GetCreateRequestHelp()
        {
            return
                "Both Employee and Manager roles can raise requests.\n" +
                "Steps:\n" +
                "1. Click Requests in the sidebar.\n" +
                "2. Click Create Request.\n" +
                "3. Select a category, enter title, description, and set priority.\n" +
                "4. Submit — you will receive a request number.\n" +
                "Note: If you are an Employee and the category requires approval, the request moves to Pending Approval automatically. If you are a Manager, it always goes to Open regardless of the category setting.";
        }

        private static string GetExplainStatusHelp()
        {
            return
                "Request statuses in order:\n" +
                "1. Open — just created, not yet assigned\n" +
                "2. Pending Approval — waiting for Manager to approve (Employee requests only, when category requires approval)\n" +
                "3. Approved — Manager approved, waiting for Admin to assign\n" +
                "4. Rejected — Manager rejected (terminal, no further action possible)\n" +
                "5. Assigned — Admin assigned a Support user\n" +
                "6. In Progress — Support has started working\n" +
                "7. Resolved — Support marked it resolved\n" +
                "8. Closed — auto-closed by system after resolved (no manual step needed)";
        }

        private static string GetGreetingMessage(string role) =>
            "Hello! I am FlowDesk AI, your helpdesk assistant. I can help you with requests, statuses, SLA, roles, and navigating the system. Type 'help' to see everything I can do.";

        private static string GetHowToApprove(string userRole) => userRole switch
        {
            "Manager" =>
                "As a Manager, you can approve or reject requests in Pending Approval status from your own team.\n" +
                "To APPROVE:\n" +
                "1. Go to Requests, open a Pending Approval request.\n" +
                "2. Click Approve.\n" +
                "3. Optionally select a remark and add a comment.\n" +
                "4. Confirm. Status moves to Approved.\n" +
                "To REJECT:\n" +
                "1. Click Reject on a Pending Approval request.\n" +
                "2. Select a remark (required) and type a comment (required).\n" +
                "3. Confirm. Status moves to Rejected permanently.\n" +
                "Note: You can only act on requests from your direct team. Admin cannot approve — that is your responsibility.",

            "Admin" =>
                "Admin cannot approve or reject requests — that is the Manager's responsibility.\n" +
                "As Admin your actions are:\n" +
                "- Assign approved (or open) requests to Support users\n" +
                "- Escalate active requests\n" +
                "- Manage users and categories\n" +
                "If a request is stuck in Pending Approval, the employee's Manager must act on it.",

            "Employee" =>
                "Employees cannot approve requests. When you raise a request that requires approval, your Manager will review and approve or reject it.\n" +
                "You will see the status update on your request automatically.",

            "Support" =>
                "Support users cannot approve or reject requests. Your responsibility is to work on requests assigned to you and update their status.",

            _ => "Only Managers can approve or reject requests."
        };

        private static string GetHowToAssign(string userRole) => userRole switch
        {
            "Admin" =>
                "As Admin, you assign requests to Support users.\n" +
                "Steps:\n" +
                "1. Go to Requests in the sidebar.\n" +
                "2. Find a request with status Approved or Open.\n" +
                "   (Approved = Employee's request that passed Manager approval)\n" +
                "   (Open = Manager-created request, or no-approval category)\n" +
                "3. Click the Assign button.\n" +
                "4. Select a Support user.\n" +
                "5. Optionally add a remark and comment, then confirm.\n" +
                "Status moves to Assigned.\n" +
                "Note: You cannot assign requests in Pending Approval, Rejected, In Progress, Resolved, or Closed state.",

            "Manager" =>
                "Managers cannot assign requests to Support users — only Admin can assign.\n" +
                "Your role is to approve or reject Pending Approval requests from your team.\n" +
                "After you approve, the Admin will assign it to a Support user.",

            "Employee" =>
                "Employees cannot assign requests. After your Manager approves your request, the Admin will assign it to a Support user who will work on it.",

            "Support" =>
                "Support users cannot assign requests. The Admin assigns requests to you. Your assigned requests appear in your Requests page.",

            _ => "Only Admin can assign requests."
        };

        private static string GetHowToComment(string userRole) => userRole switch
        {
            "Manager" =>
                "As a Manager, you can add comments on requests.\n" +
                "How:\n" +
                "1. Open any request from your team.\n" +
                "2. Scroll to the Comments section at the bottom.\n" +
                "3. Type your comment and click Add Comment.\n" +
                "When approving: a comment is optional.\n" +
                "When rejecting: a comment is mandatory — it becomes the rejection reason on record.\n" +
                "Note: Employees cannot add comments. They can only view comments on their own requests.",

            "Admin" =>
                "As Admin, you can add comments on any request at any time.\n" +
                "How:\n" +
                "1. Open any request.\n" +
                "2. Scroll to the Comments section.\n" +
                "3. Type your comment and click Add Comment.\n" +
                "You can also add a comment during assignment. Employees cannot comment.",

            "Support" =>
                "As a Support user, you can add comments on requests assigned to you.\n" +
                "How:\n" +
                "1. Open a request assigned to you.\n" +
                "2. Scroll to the Comments section.\n" +
                "3. Type your comment and click Add Comment.\n" +
                "When marking Resolved, you can optionally add a resolution note as a comment.",

            "Employee" =>
                "Employees cannot add comments on requests.\n" +
                "You can view comments that your Manager, Admin, or Support user have added on your requests.",

            _ => "Comments can be added by Manager, Admin, and Support users only. Employees cannot comment."
        };

        private static string GetHowToEscalate(string userRole) => userRole switch
        {
            "Admin" =>
                "As Admin, you can escalate requests that need urgent attention.\n" +
                "Steps:\n" +
                "1. Go to Requests in the sidebar.\n" +
                "2. Open an active request (not Pending Approval, Resolved, or Closed).\n" +
                "3. Click the Escalate button in the action area.\n" +
                "4. Enter an escalation reason (required — cannot be blank).\n" +
                "5. Confirm.\n" +
                "The request will show an escalation warning badge in the list.\n" +
                "Important: A request can only be escalated once. If it is already escalated, the Escalate button will not appear.",

            _ =>
                $"Only Admin can escalate requests. As a {userRole}, you cannot escalate.\n" +
                "If a request needs urgent attention, contact your Admin and they can escalate it."
        };

        private static string GetHowToUpdateStatus(string userRole) => userRole switch
        {
            "Support" =>
                "As a Support user, you can update the status of requests assigned specifically to you.\n" +
                "Allowed transitions:\n" +
                "- Assigned to In Progress: click Start on the request.\n" +
                "- In Progress to Resolved: click Resolve, optionally add a remark and resolution comment.\n" +
                "After you mark Resolved, the system automatically closes the request — you do not need to close it manually.\n" +
                "Note: You can only move status forward, never backwards. You cannot update status on requests not assigned to you.",

            "Admin" =>
                "Admin cannot directly move requests through the Support workflow (that is the assigned Support user's responsibility).\n" +
                "Admin actions on requests are:\n" +
                "- Assign (Open or Approved) to a Support user\n" +
                "- Escalate active requests\n" +
                "- Add comments at any time\n" +
                "To have a request progressed, ensure it is assigned to a Support user.",

            _ =>
                $"As a {userRole}, you cannot update request status.\n" +
                "Status is updated by the Support user assigned to the request:\n" +
                "- Assigned to In Progress (when they start work)\n" +
                "- In Progress to Resolved (when done)\n" +
                "Resolved requests are automatically closed by the system."
        };

        private static string GetExplainRoles()
        {
            return
                "FlowDesk has 4 roles:\n\n" +
                "Employee:\n" +
                "- Raise new requests (category, title, description, priority)\n" +
                "- View only their own requests\n" +
                "- Cannot approve, assign, escalate, or comment\n\n" +
                "Manager:\n" +
                "- Raise requests (same as Employee)\n" +
                "- Approve or reject Pending Approval requests from direct team only\n" +
                "- View all requests from their team\n" +
                "- Comment is optional on approve, mandatory on reject\n" +
                "- Cannot assign to Support or escalate\n\n" +
                "Admin:\n" +
                "- Assign approved (or open) requests to Support users\n" +
                "- Escalate any active request once (mandatory reason required)\n" +
                "- Manage users and categories\n" +
                "- View all requests across the system\n" +
                "- Add comments on any request at any time\n" +
                "- Cannot approve or reject (that is Manager only)\n\n" +
                "Support:\n" +
                "- View only requests assigned to them\n" +
                "- Move status: Assigned to In Progress, then In Progress to Resolved\n" +
                "- Add comments on assigned requests\n" +
                "- Cannot approve, reject, assign, or escalate";
        }

        private static string Truncate(string value, int maxLength)
        {
            return value?.Length > maxLength ? value[..maxLength] + "..." : value ?? "";
        }
    }
}

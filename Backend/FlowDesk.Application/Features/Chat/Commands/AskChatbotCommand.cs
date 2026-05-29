using FlowDesk.Application.Features.Chat.DTOs;
using FlowDesk.Application.Features.Chat.Enums;
using FlowDesk.Application.Features.Chat.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Chat.Commands
{
    public record AskChatbotCommand(string Message, int UserId, string UserRole) : IRequest<ChatResponseDto>;

    public class AskChatbotCommandHandler(
        ICommandResolver commandResolver,
        IEntityContextBuilder contextBuilder,
        IChatbotService chatbotService,
        IConversationMemory conversationMemory,
        IRateLimiter rateLimiter,
        IGroundingGuard groundingGuard,
        ILogger<AskChatbotCommandHandler> logger)
        : IRequestHandler<AskChatbotCommand, ChatResponseDto>
    {
        private const int MaxAiCallsPerMinute = 10;

        private static readonly string SystemPrompt =
  "You are Flowdesk AI \u2014 a helpdesk assistant " +
  "for FlowDesk Service Request Management System.\n\n" +

  "SYSTEM KNOWLEDGE:\n" +
  "FlowDesk has 4 roles with strict boundaries:\n" +
  "- Employee: Can raise requests and view " +
    "their own requests only. Cannot approve, " +
    "assign, comment, or escalate.\n" +
  "- Manager: Can approve/reject requests from " +
    "their direct team. Can view team requests. " +
    "Cannot assign to Support or escalate.\n" +
  "- Admin: Can assign approved requests to " +
    "Support users. Can escalate, manage users " +
    "and categories. Cannot approve requests " +
    "(that is Manager\u2019s job only).\n" +
  "- Support: Can update status of requests " +
    "assigned to them. Can add resolution notes " +
    "as comments. Cannot approve or assign.\n\n" +

  "REQUEST LIFECYCLE:\n" +
  "1. Employee or Manager raises \u2192 Status: Open\n" +
  "2. Employee + approval-required category \u2192 Pending Approval\n" +
  "3. Manager approves \u2192 Approved\n" +
  "4. Admin assigns \u2192 Assigned\n" +
  "5. Support starts \u2192 In Progress\n" +
  "6. Support resolves \u2192 Resolved\n" +
  "7. Final \u2192 Closed\n" +
  "Rejected is a terminal state from step 3.\n\n" +

  "COMMENT RULES:\n" +
  "Only Manager, Admin, and Support can comment. " +
  "Employees cannot comment. Rejection requires " +
  "BOTH a remark (dropdown) AND a comment (text) — " +
  "both mandatory. Approval comment is optional. " +
  "Support comment optional when resolving.\n\n" +

  "SLA RULES:\n" +
  "Every category has SLA hours. DueDate is set " +
  "when request is created. SLA status is:\n" +
  "- Within SLA: time remaining\n" +
  "- Nearing Breach: less than 2 hours left\n" +
  "- Breached: due date passed, not resolved\n" +
  "- Completed: resolved or closed\n\n" +

  "ESCALATION RULES:\n" +
  "Only Admin can escalate. A request can only " +
  "be escalated once. Cannot escalate Resolved " +
  "or Closed requests. Escalation requires a " +
  "mandatory reason.\n\n" +

  "UI NAVIGATION:\n" +
  "- Sidebar: Dashboard, Requests, Categories " +
    "(Admin only), Users (Admin only)\n" +
  "- Requests page: list with filters, tabs, " +
    "search by status/priority/category\n" +
  "- Request detail: shows status history, " +
    "comments, SLA info, action buttons\n" +
  "- Action buttons shown based on role + status\n" +
  "- Dashboard: shows role-specific stats " +
    "and charts\n\n" +

  "STRICT RULES:\n" +
  "- Answer ONLY based on provided context data\n" +
  "- If data not in context: say exactly: " +
    "I don\u2019t have that information right now\n" +
  "- Never invent steps, buttons, or menus " +
    "that may not exist\n" +
  "- Never reveal data of other users\n" +
  "- Keep responses concise (3-4 lines max)\n" +
  "- If user asks about an action they cannot " +
    "do, explain why and who can do it instead\n" +
  "- Never use markdown, bullets with *, or **";

        public async Task<ChatResponseDto> Handle(AskChatbotCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Chat: UserId={UserId} Role={Role} Len={Len}",
                request.UserId, request.UserRole, request.Message.Length);

            var sanitized = SanitizeInput(request.Message);

            // Step 1: Resolve intent
            var intent = await commandResolver.ResolveAsync(sanitized, request.UserId, request.UserRole, cancellationToken);

            // Step 2: If concrete intent, handle via CQRS - no AI call
            if (intent.IsResolved && intent.Intent != IntentType.GeneralQuery)
            {
                var result = await commandResolver.HandleIntentAsync(intent, request.UserId, request.UserRole, cancellationToken);
                if (result != null)
                {
                    logger.LogInformation("Intent {Intent} handled without AI for UserId={UserId}", intent.Intent, request.UserId);
                    return new ChatResponseDto { Response = result, IsCommandHandled = true };
                }
            }

            // Step 3: AI path - check rate limit
            if (!rateLimiter.IsAllowed(request.UserId, MaxAiCallsPerMinute, TimeSpan.FromMinutes(1)))
            {
                logger.LogWarning("Rate limit exceeded for UserId={UserId}", request.UserId);
                return new ChatResponseDto
                {
                    Response = "Too many requests, please try again shortly.",
                    IsCommandHandled = false
                };
            }

            // Step 4: Build minimal context
            var contextData = await contextBuilder.BuildContextAsync(request.UserId, request.UserRole, intent, cancellationToken);

            // Step 5: Get conversation history
            var history = conversationMemory.GetHistory(request.UserId);

            // Step 6: Determine max tokens based on input complexity
            var maxTokens = EstimateTokens(sanitized);

            // Step 7: Call AI
            var aiResponse = await chatbotService.GetResponseAsync(sanitized, SystemPrompt, contextData, history, maxTokens, cancellationToken);

            // Step 8: Grounding guard
            var validated = groundingGuard.Validate(aiResponse, contextData);

            // Step 9: Store in conversation memory
            conversationMemory.AddMessage(request.UserId, "user", sanitized);
            conversationMemory.AddMessage(request.UserId, "assistant", validated);

            logger.LogInformation("AI response sent to UserId={UserId} Tokens={Tokens}", request.UserId, maxTokens);
            return new ChatResponseDto { Response = validated, IsCommandHandled = false };
        }

        private static int EstimateTokens(string message)
        {
            var length = message.Length;
            if (length < 20) return 150;
            if (length < 60) return 250;
            if (length < 150) return 400;
            return 600;
        }

        private static string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var sanitized = input
                .Replace("ignore previous instructions", "", StringComparison.OrdinalIgnoreCase)
                .Replace("ignore all rules", "", StringComparison.OrdinalIgnoreCase)
                .Replace("forget everything", "", StringComparison.OrdinalIgnoreCase)
                .Replace("you are not", "I am", StringComparison.OrdinalIgnoreCase)
                .Replace("act as", "", StringComparison.OrdinalIgnoreCase);

            return sanitized.Trim();
        }
    }
}

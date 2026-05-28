using FlowDesk.Application.Features.Chat.DTOs;
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
        ILogger<AskChatbotCommandHandler> logger)
        : IRequestHandler<AskChatbotCommand, ChatResponseDto>
    {
        private static readonly string SystemPrompt = $@"
You are an enterprise AI assistant for a Request Management System.
You MUST follow these rules:

- Answer ONLY based on system context provided.
- Do NOT hallucinate or assume data.
- Respect user role permissions strictly.
- If user is Employee -> only their requests.
- Manager -> team requests.
- Admin -> all access.
- Support -> assigned requests.

You can:
- Explain request status.
- Help create requests.
- Answer system usage questions.
- Guide users step-by-step.

If question is unclear -> ask clarification.
If data not available -> say 'I don't have that information'.

Always respond professionally and concisely.
";

        public async Task<ChatResponseDto> Handle(AskChatbotCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Chat query from UserId {UserId} with Role {Role}: {Message}",
                request.UserId, request.UserRole, request.Message);

            var sanitizedMessage = SanitizeInput(request.Message);

            var resolvedCommand = await commandResolver.ResolveAsync(sanitizedMessage, request.UserId, request.UserRole, cancellationToken);
            if (resolvedCommand != null)
            {
                logger.LogInformation("Command resolved for UserId {UserId}: {Response}", request.UserId, resolvedCommand);
                return new ChatResponseDto
                {
                    Response = resolvedCommand,
                    IsCommandHandled = true
                };
            }

            var contextData = await contextBuilder.BuildContextAsync(request.UserId, request.UserRole, cancellationToken);

            var aiResponse = await chatbotService.GetResponseAsync(sanitizedMessage, SystemPrompt, contextData, cancellationToken);

            logger.LogInformation("AI response sent to UserId {UserId}", request.UserId);
            return new ChatResponseDto
            {
                Response = aiResponse,
                IsCommandHandled = false
            };
        }

        private static string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

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

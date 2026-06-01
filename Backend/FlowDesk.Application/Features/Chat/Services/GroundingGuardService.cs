using System.Text.RegularExpressions;
using FlowDesk.Application.Features.Chat.Interfaces;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Chat.Services
{
    public class GroundingGuardService(ILogger<GroundingGuardService> logger) : IGroundingGuard
    {
        public string Validate(string aiResponse, string contextData)
        {
            if (string.IsNullOrWhiteSpace(aiResponse))
                return "No response available.";

            var mentionedReqs = Regex.Matches(aiResponse, @"REQ[-\s]?\d+[-\w]*", RegexOptions.IgnoreCase)
                .Select(m => m.Value.ToUpper().Replace(" ", "-"))
                .ToHashSet();

            if (mentionedReqs.Count == 0)
                return aiResponse;

            foreach (var req in mentionedReqs)
            {
                if (!contextData.Contains(req, StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogWarning("Grounding guard: AI mentioned {Req} not found in context", req);
                    return Regex.Replace(aiResponse, Regex.Escape(req), "[request not found]", RegexOptions.IgnoreCase);
                }
            }

            return aiResponse;
        }
    }
}

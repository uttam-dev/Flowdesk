using FlowDesk.Application.Features.Chat.DTOs;
using FlowDesk.Application.Features.Chat.Enums;
using FlowDesk.Application.Features.Requests.Commands;
using FlowDesk.Application.Features.Requests.DTOs;
using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Chat.Services
{
    public class ActionExecutorService(
        IMediator mediator,
        ICategoryRepository categoryRepository,
        IRequestRepository requestRepository,
        ILogger<ActionExecutorService> logger)
    {
        public async Task<string> ExecuteAsync(IntentResult intent, int userId, string userRole, CancellationToken ct)
        {
            return intent.Intent switch
            {
                IntentType.CreateRequest => await HandleCreateRequest(intent, userId, userRole, ct),
                IntentType.ApproveRequest => await HandleApproveRequest(intent, userId, userRole, ct),
                IntentType.RejectRequest => await HandleRejectRequest(intent, userId, userRole, ct),
                IntentType.StartRequest => await HandleStartRequest(intent, userId, userRole, ct),
                IntentType.ResolveRequest => await HandleResolveRequest(intent, userId, userRole, ct),
                _ => "I could not process that action."
            };
        }

        private async Task<string> HandleCreateRequest(IntentResult intent, int userId, string userRole, CancellationToken ct)
        {
            if (userRole is not ("Employee" or "Manager"))
                return "Only Employee and Manager can create requests.";

            intent.Parameters.TryGetValue("title", out var title);
            intent.Parameters.TryGetValue("description", out var description);
            intent.Parameters.TryGetValue("categoryHint", out var categoryHint);
            intent.Parameters.TryGetValue("priority", out var priorityStr);

            if (string.IsNullOrWhiteSpace(title))
                return "Please tell me the title of the issue. Example: 'create request for VPN not connecting'";

            var allCats = await categoryRepository.GetAllAsync(
                new Domain.DTOs.FilterCategoryDataQueryDto { PageNumber = 1, PageSize = 50 });

            var activeCategories = allCats.Item2.Where(c => c.IsActive).ToList();
            if (!activeCategories.Any())
                return "No active categories found. Please contact Admin.";

            Domain.Entities.Category? matchedCat = null;
            if (!string.IsNullOrWhiteSpace(categoryHint))
            {
                matchedCat = activeCategories.FirstOrDefault(c =>
                    c.CategoryName.Contains(categoryHint, StringComparison.OrdinalIgnoreCase));
            }
            matchedCat ??= activeCategories.FirstOrDefault(c =>
                c.CategoryName.Contains("general", StringComparison.OrdinalIgnoreCase) ||
                c.CategoryName.Contains("IT", StringComparison.OrdinalIgnoreCase))
                ?? activeCategories.First();

            var priority = priorityStr?.ToLower() switch
            {
                "high" or "urgent" or "critical" => PriorityEnum.High,
                "low" or "minor" => PriorityEnum.Low,
                _ => PriorityEnum.Medium
            };

            var dto = new CreateRequestDto
            {
                CategoryId = matchedCat.CategoryId,
                Title = title,
                Description = description ?? title,
                Priority = priority
            };

            try
            {
                var result = await mediator.Send(new CreateRequestCommand(userRole, userId, dto), ct);

                var status = matchedCat.IsApprovalRequired && userRole == "Employee"
                    ? "Pending Approval (your Manager will review)"
                    : "Open";

                return $"Request created successfully!\n" +
                       $"Number: {result.RequestNumber}\n" +
                       $"Title: {result.Title}\n" +
                       $"Category: {matchedCat.CategoryName}\n" +
                       $"Priority: {priority}\n" +
                       $"Status: {status}";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Create request failed for UserId={UserId}", userId);
                return "Failed to create the request. Please try again or use the Requests page.";
            }
        }

        private async Task<string> HandleApproveRequest(IntentResult intent, int userId, string userRole, CancellationToken ct)
        {
            if (userRole != "Manager")
                return "Only Managers can approve requests.";

            if (!intent.Parameters.TryGetValue("reqNumber", out var reqNumber))
                return "Please provide the request number. Example: 'approve REQ-20260529-001234'";

            var req = await requestRepository.GetByRequestNumberAsync(reqNumber.ToUpper());
            if (req == null) return $"Request {reqNumber} not found.";
            if (req.Status != RequestStatusEnum.PendingApproval)
                return $"Request {reqNumber} is '{req.Status}' — only Pending Approval requests can be approved.";

            intent.Parameters.TryGetValue("comment", out var comment);

            try
            {
                await mediator.Send(new ApproveRequestCommand(req.RequestId, userId,
                    new RemarkDto { MasterRemarkId = 0, CommentText = comment }), ct);

                return $"Request {reqNumber} approved successfully.\n" +
                       $"Title: {req.Title}\n" +
                       $"Status: Approved — Admin will now assign it to Support.";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Approve failed for {ReqNumber}", reqNumber);
                return "Approval failed. You can only approve requests from your own team.";
            }
        }

        private async Task<string> HandleRejectRequest(IntentResult intent, int userId, string userRole, CancellationToken ct)
        {
            if (userRole != "Manager")
                return "Only Managers can reject requests.";

            if (!intent.Parameters.TryGetValue("reqNumber", out var reqNumber))
                return "Please provide the request number. Example: 'reject REQ-20260529-001234 reason: duplicate issue'";

            if (!intent.Parameters.TryGetValue("reason", out var reason) || string.IsNullOrWhiteSpace(reason))
                return $"Please provide a rejection reason. Example: 'reject {reqNumber} reason: duplicate issue'";

            var req = await requestRepository.GetByRequestNumberAsync(reqNumber.ToUpper());
            if (req == null) return $"Request {reqNumber} not found.";
            if (req.Status != RequestStatusEnum.PendingApproval)
                return $"Request {reqNumber} is '{req.Status}' — only Pending Approval requests can be rejected.";

            try
            {
                await mediator.Send(new RejectRequestCommand(req.RequestId, userId,
                    new RemarkDto { MasterRemarkId = 1, CommentText = reason }), ct);

                return $"Request {reqNumber} rejected.\n" +
                       $"Title: {req.Title}\n" +
                       $"Reason recorded: {reason}";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Reject failed for {ReqNumber}", reqNumber);
                return "Rejection failed. You can only reject requests from your own team.";
            }
        }

        private async Task<string> HandleStartRequest(IntentResult intent, int userId, string userRole, CancellationToken ct)
        {
            if (userRole != "Support")
                return "Only Support users can start working on requests.";

            if (!intent.Parameters.TryGetValue("reqNumber", out var reqNumber))
                return "Please provide the request number. Example: 'start REQ-20260529-001234'";

            var req = await requestRepository.GetByRequestNumberAsync(reqNumber.ToUpper());
            if (req == null) return $"Request {reqNumber} not found.";
            if (req.AssignedToId != userId)
                return $"Request {reqNumber} is not assigned to you.";
            if (req.Status != RequestStatusEnum.Assigned)
                return $"Request {reqNumber} is '{req.Status}' — only Assigned requests can be started.";

            try
            {
                await mediator.Send(new UpdateRequestStatusCommand(req.RequestId, userId, userRole,
                    new UpdateRequestStatusDto { Status = RequestStatusEnum.InProgress }), ct);

                return $"Request {reqNumber} is now In Progress.\nTitle: {req.Title}";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Start failed for {ReqNumber}", reqNumber);
                return "Could not start the request. Please try from the Requests page.";
            }
        }

        private async Task<string> HandleResolveRequest(IntentResult intent, int userId, string userRole, CancellationToken ct)
        {
            if (userRole != "Support")
                return "Only Support users can resolve requests.";

            if (!intent.Parameters.TryGetValue("reqNumber", out var reqNumber))
                return "Please provide the request number. Example: 'resolve REQ-20260529-001234 note: issue fixed'";

            var req = await requestRepository.GetByRequestNumberAsync(reqNumber.ToUpper());
            if (req == null) return $"Request {reqNumber} not found.";
            if (req.AssignedToId != userId)
                return $"Request {reqNumber} is not assigned to you.";
            if (req.Status != RequestStatusEnum.InProgress)
                return $"Request {reqNumber} is '{req.Status}' — only In Progress requests can be resolved.";

            intent.Parameters.TryGetValue("note", out var note);

            try
            {
                await mediator.Send(new UpdateRequestStatusCommand(req.RequestId, userId, userRole,
                    new UpdateRequestStatusDto
                    {
                        Status = RequestStatusEnum.Resolved,
                        CommentText = note
                    }), ct);

                return $"Request {reqNumber} resolved and auto-closed.\n" +
                       $"Title: {req.Title}\n" +
                       (note != null ? $"Resolution note saved." : "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Resolve failed for {ReqNumber}", reqNumber);
                return "Could not resolve the request. Please try from the Requests page.";
            }
        }
    }
}

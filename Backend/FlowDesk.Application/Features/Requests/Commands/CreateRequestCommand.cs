using AutoMapper;
using FlowDesk.Application.Features.Requests.DTOs;
using FlowDesk.Application.Features.Users.DTOs;
using FlowDesk.Application.Services;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Interfaces;
using FlowDesk.Domain.Utils;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Requests.Commands
{
    public record CreateRequestCommand(string CurrentUserRole, int CurrentUserId, CreateRequestDto Dto) : IRequest<EmployeeRequestResponseDto>;

    public class CreateRequestCommandHandler(
      IRequestRepository requestRepository,
      ICategoryRepository categoryRepository,
      IUserRepository userRepository,
      IRequestHistoryRepository requestHistoryRepository,
      IMapper mapper,
      ILogger<CreateRequestCommandHandler> logger)
      : IRequestHandler<CreateRequestCommand, EmployeeRequestResponseDto>
    {
        public async Task<EmployeeRequestResponseDto> Handle(
            CreateRequestCommand request,
            CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting {Operation} with {@Request}", nameof(CreateRequestCommandHandler), request);

            if (request.CurrentUserRole is not (nameof(RoleEnum.Employee) or nameof(RoleEnum.Manager)))
            {
                logger.LogWarning("Unauthorized role {Role} tried to create request", request.CurrentUserRole);
                throw new Common.Exceptions.UnauthorizedException("Only employees and manager can create requests.");
            }

            var category = await categoryRepository.GetByIdAsync(request.Dto.CategoryId);
            if (category == null)
            {
                throw new Common.Exceptions.NotFoundException("Category not found.");
            }

            var newRequest = mapper.Map<Request>(request.Dto);
            newRequest.RequestNumber = RequestNumberGenerator.Generate();
            newRequest.EmployeeId = request.CurrentUserId;

            // STATUS DECISION
            bool isEmployee = request.CurrentUserRole == RoleEnum.Employee.ToString();
            bool approvalRequired = category.IsApprovalRequired;

            if (isEmployee && approvalRequired)
            {
                newRequest.Status = RequestStatusEnum.PendingApproval;
            }
            else
            {
                newRequest.Status = RequestStatusEnum.Open;
            }

            var createdReq = await requestRepository.AddAsync(newRequest);

            var slaHours = category.SLAHours > 0 ? category.SLAHours : 24;
            createdReq.DueDate = createdReq.CreatedOn.AddHours(slaHours);
            await requestRepository.Update(createdReq);

            // ================= HISTORY =================

            // 1. Always first history: NULL -> OPEN
            await requestHistoryRepository.AddAsync(new Domain.Entities.RequestHistory
            {
                RequestId = createdReq.RequestId,
                OldStatus = null,
                NewStatus = RequestStatusEnum.Open,
                ChangedById = request.CurrentUserId
            });

            // 2. Employee + approval required → OPEN -> PENDING
            if (isEmployee && approvalRequired)
            {
                await requestHistoryRepository.AddAsync(new Domain.Entities.RequestHistory
                {
                    RequestId = createdReq.RequestId,
                    OldStatus = RequestStatusEnum.Open,
                    NewStatus = RequestStatusEnum.PendingApproval,
                    ChangedById = null,
                    IsSystemGenerated = true
                });
            }

            // ================= RESPONSE =================

            var response = mapper.Map<EmployeeRequestResponseDto>(createdReq);
            response.CategoryName = category.CategoryName;

            if (approvalRequired)
            {
                var manager = await userRepository.GetManagerByEmployeeId(request.CurrentUserId);
                response.ApprovalName = manager?.FullName;
            }

            logger.LogInformation("Completed {Operation} for RequestId {RequestId}", createdReq.RequestId);

            return response;
        }
    }
}

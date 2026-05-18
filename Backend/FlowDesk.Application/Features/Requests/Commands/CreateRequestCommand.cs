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

            if (request.CurrentUserRole is not (RoleName.Employee or RoleName.Manager))
            {
                logger.LogWarning("Unauthorized role {Role} tried to create request", request.CurrentUserRole);
                throw new Common.Exceptions.UnauthorizedException("Only employees and manager can create requests.");
            }

            logger.LogInformation("Fetching Category with Id {CategoryId}", request.Dto.CategoryId);
            var category = await categoryRepository.GetByIdAsync(request.Dto.CategoryId);

            if (category == null)
            {
                logger.LogWarning("Category not found with Id {CategoryId}", request.Dto.CategoryId);
                throw new Common.Exceptions.NotFoundException("Category not found.");
            }

            logger.LogInformation("Creating new Request for UserId {UserId}", request.CurrentUserId);

            var newRequest = mapper.Map<Request>(request.Dto);
            newRequest.RequestNumber = RequestNumberGenerator.Generate();
            newRequest.EmployeeId = request.CurrentUserId;

            if (request.CurrentUserRole is RoleName.Employee && category.IsApprovalRequired == true)
            {
                newRequest.Status = RequestStatusEnum.PendingApproval;
            }
            else
            {
                newRequest.Status = RequestStatusEnum.Open;
            }

            var createdReq = await requestRepository.AddAsync(newRequest);

            logger.LogInformation("Request created with Id {RequestId} and Status {Status}", createdReq.RequestId, createdReq.Status);

            await requestHistoryRepository.AddAsync(new Domain.Entities.RequestHistory
            {
                RequestId = createdReq.RequestId,
                OldStatus = request.CurrentUserRole == RoleName.Employee
                    ? RequestStatusEnum.Open
                    : null,
                NewStatus = createdReq.Status,
                IsSystemGenerated = true
            });

            var response = mapper.Map<EmployeeRequestResponseDto>(createdReq);
            response.CategoryName = category.CategoryName;

            if (category.IsApprovalRequired == true)
            {
                logger.LogInformation("Fetching Manager for UserId {UserId}", request.CurrentUserId);
                var manager = await userRepository.GetManagerByEmployeeId(request.CurrentUserId);
                response.ApprovalName = manager?.FullName;
            }

            logger.LogInformation("Completed {Operation} for RequestId {RequestId}", nameof(CreateRequestCommandHandler), createdReq.RequestId);

            return response;
        }
    }
}
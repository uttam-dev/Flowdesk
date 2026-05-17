using AutoMapper;
using FlowDesk.Application.Features.Requests.DTOs;
using FlowDesk.Application.Features.Users.DTOs;
using FlowDesk.Application.Services;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Interfaces;
using FlowDesk.Domain.Utils;
using MediatR;

namespace FlowDesk.Application.Features.Requests.Commands
{
    public record CreateRequestCommand(string CurrentUserRole, int CurrentUserId, CreateRequestDto Dto) : IRequest<EmployeeRequestResponseDto>;

    public class CreateRequestCommandHandler(
      IRequestRepository requestRepository,
      ICategoryRepository categoryRepository,
      IUserRepository userRepository,
      IRequestHistoryRepository requestHistoryRepository,
      IMapper mapper)
      : IRequestHandler<CreateRequestCommand, EmployeeRequestResponseDto>
    {
        public async Task<EmployeeRequestResponseDto> Handle(
            CreateRequestCommand request,
            CancellationToken cancellationToken)
        {
            if (request.CurrentUserRole is not (RoleName.Employee or RoleName.Manager))
            {
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

            //Set initial status based on role
            if (request.CurrentUserRole is RoleName.Employee)
            {
                newRequest.Status = RequestStatusEnum.PendingApproval;
            }
            else // Manager
            {
                newRequest.Status = RequestStatusEnum.Open;
            }

            var createdReq = await requestRepository.AddAsync(newRequest);

            // Add History (for both roles)
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

            var manager = await userRepository.GetManagerByEmployeeId(request.CurrentUserId);
            response.ApprovalName = manager?.FullName;

            return response;
        }
    }
}
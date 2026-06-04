using AutoMapper;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Features.Requests.DTOs;
using FlowDesk.Application.Services;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Requests.Commands
{
    public record EscalateRequestCommand(int RequestId, int AdminUserId, EscalateRequestDto Dto) : IRequest<RequestResponseDto>;

    public class EscalateRequestCommandHandler(
        IRequestRepository requestRepository,
        IMapper mapper,
        ISlaService slaService,
        ILogger<EscalateRequestCommandHandler> logger)
        : IRequestHandler<EscalateRequestCommand, RequestResponseDto>
    {
        public async Task<RequestResponseDto> Handle(EscalateRequestCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting {Operation} for RequestId {RequestId}", nameof(EscalateRequestCommandHandler), request.RequestId);

            if (string.IsNullOrWhiteSpace(request.Dto.EscalationReason))
            {
                throw new BadRequestException("EscalationReason is required");
            }

            var requestResult = await requestRepository.GetByIdWithEscalationAsync(request.RequestId);
            if (requestResult == null)
            {
                throw new NotFoundException($"Request with ID {request.RequestId} not found.");
            }

            if (requestResult.Status == RequestStatusEnum.PendingApproval ||
                requestResult.Status == RequestStatusEnum.Resolved ||
                requestResult.Status == RequestStatusEnum.Closed)
            {
                throw new BadRequestException("Cannot escalate this request");
            }

            if (requestResult.IsEscalated)
            {
                throw new ConflictException("Request is already escalated");
            }

            var currentTime = DateTime.UtcNow;

            requestResult.IsEscalated = true;
            requestResult.EscalatedOn = currentTime;
            requestResult.EscalatedBy = request.AdminUserId;
            requestResult.EscalationReason = request.Dto.EscalationReason;
            requestResult.UpdatedOn = currentTime;

            await requestRepository.AddEscalationHistoryAsync(new EscalationHistory
            {
                RequestId = requestResult.RequestId,
                EscalatedBy = request.AdminUserId,
                EscalatedOn = currentTime,
                EscalationReason = request.Dto.EscalationReason,
                CreatedOn = currentTime
            });

            await requestRepository.Update(requestResult);

            var updatedRequest = await requestRepository.GetByIdWithEscalationAsync(request.RequestId) ?? requestResult;
            var response = mapper.Map<RequestResponseDto>(updatedRequest);
            response.SlaStatus = slaService.CalculateSlaStatus(response.DueDate, response.Status.ToString());

            logger.LogInformation("Completed {Operation} for RequestId {RequestId}", nameof(EscalateRequestCommandHandler), request.RequestId);

            return response;
        }
    }
}

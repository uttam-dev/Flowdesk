using AutoMapper;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Features.Requests.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlowDesk.Application.Features.Requests.Queries
{
    public record GetRequestByIdQuery(int userId) : IRequest<RequestResponseDto>;

    public class GetRequestByIdQueryHandler(
       IRequestRepository requestRepository,
       IMapper mapper,
       ILogger<GetRequestByIdQueryHandler> logger)
       : IRequestHandler<GetRequestByIdQuery, RequestResponseDto>
    {
        public async Task<RequestResponseDto> Handle(GetRequestByIdQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting {Operation} for RequestId {RequestId}",
                nameof(GetRequestByIdQueryHandler), request.userId);

            var requestResult = await requestRepository.GetByIdAsync(request.userId);

            if (requestResult == null)
            {
                logger.LogWarning("Request not found with Id {RequestId}", request.userId);
                throw new NotFoundException($"Request with ID {request.userId} not found.");
            }

            if (requestResult.Category!.IsApprovalRequired == false)
            {
                requestResult.Employee?.Manager = null;
            }

            logger.LogInformation("Mapping RequestId {RequestId} to DTO", request.userId);

            var result = mapper.Map<RequestResponseDto>(requestResult);

            logger.LogInformation("Completed {Operation} for RequestId {RequestId}",
                nameof(GetRequestByIdQueryHandler), request.userId);

            return result;
        }
    }
}
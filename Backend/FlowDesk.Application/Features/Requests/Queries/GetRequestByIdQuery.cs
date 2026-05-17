using FlowDesk.Application.Features.Requests.DTOs;
using MediatR;
using AutoMapper;
using FlowDesk.Domain.Interfaces;
using FlowDesk.Application.Common.Exceptions;

namespace FlowDesk.Application.Features.Requests.Queries
{
    public record GetRequestByIdQuery(int userId) : IRequest<RequestResponseDto>;

    public class GetRequestByIdQueryHandler(IRequestRepository requestRepository, IMapper mapper) : IRequestHandler<GetRequestByIdQuery, RequestResponseDto>
    {
        public async Task<RequestResponseDto> Handle(GetRequestByIdQuery request, CancellationToken cancellationToken)
        {
            var requestResult = await requestRepository.GetByIdAsync(request.userId);
            if (requestResult == null)
            {
                throw new NotFoundException($"Request with ID {request.userId} not found.");
            }
            return mapper.Map<RequestResponseDto>(requestResult);
        }
    }
}
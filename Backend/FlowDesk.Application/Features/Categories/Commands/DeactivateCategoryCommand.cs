using AutoMapper;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Features.Categories.DTOs;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Categories.Commands
{
    public record DeactivateCategoryCommand(int Id)
        : IRequest<CategoryResponseDto>;

    public class DeactivateCategoryCommandHandler(ICategoryRepository _repo, IMapper _mapper, ILogger<DeactivateCategoryCommandHandler> logger)
    : IRequestHandler<DeactivateCategoryCommand, CategoryResponseDto>
    {
        public async Task<CategoryResponseDto> Handle(DeactivateCategoryCommand request, CancellationToken cancellationToken)
        {
            // logging
            logger.LogInformation("Starting {Operation} with {@Request}", nameof(DeactivateCategoryCommandHandler), request);

            // logging
            logger.LogInformation("Fetching {Entity} with Id {EntityId}", "Category", request.Id);

            var category = await _repo.GetByIdAsync(request.Id);

            if (category == null)
            {
                // logging
                logger.LogWarning("{Entity} not found with Id {EntityId} in {Operation}", "Category", request.Id, nameof(DeactivateCategoryCommandHandler));
                throw new NotFoundException("Category not found");
            }

            category.IsActive = false;
            category.UpdatedOn = DateTime.UtcNow;

            // logging
            logger.LogInformation("Updating {Entity} with Id {EntityId}", "Category", request.Id);

            var updated = await _repo.Update(category);

            // logging
            logger.LogInformation("Successfully completed {Operation} for {Entity} with Id {EntityId}", nameof(DeactivateCategoryCommandHandler), "Category", request.Id);

            return _mapper.Map<CategoryResponseDto>(updated);
        }
    }
}

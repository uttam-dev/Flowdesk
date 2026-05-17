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
    public record ActivateCategoryCommand(int Id)
      : IRequest<CategoryResponseDto>;

    public class ActivateCategoryCommandHandler(ICategoryRepository _repo, IMapper _mapper, ILogger<ActivateCategoryCommandHandler> logger)
: IRequestHandler<ActivateCategoryCommand, CategoryResponseDto>
    {
        public async Task<CategoryResponseDto> Handle(ActivateCategoryCommand request, CancellationToken cancellationToken)
        {
            // logging
            logger.LogInformation("Starting {Operation} with {@Request}", nameof(ActivateCategoryCommandHandler), request);

            // logging
            logger.LogInformation("Fetching {Entity} with Id {EntityId}", "Category", request.Id);

            var category = await _repo.GetByIdAsync(request.Id);

            if (category == null)
            {
                // logging
                logger.LogWarning("{Entity} not found with Id {EntityId} in {Operation}", "Category", request.Id, nameof(ActivateCategoryCommandHandler));
                throw new NotFoundException("Category not found");
            }

            category.IsActive = true;
            category.UpdatedOn = DateTime.UtcNow;

            // logging
            logger.LogInformation("Updating {Entity} with Id {EntityId}", "Category", request.Id);

            var updated = await _repo.Update(category);

            // logging
            logger.LogInformation("Successfully completed {Operation} for {Entity} with Id {EntityId}", nameof(ActivateCategoryCommandHandler), "Category", request.Id);

            return _mapper.Map<CategoryResponseDto>(updated);
        }
    }
}

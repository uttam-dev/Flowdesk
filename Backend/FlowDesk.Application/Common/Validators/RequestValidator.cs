using FlowDesk.Domain.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Common.Validators
{
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.RequestNumber)
                .NotEmpty().WithMessage("Request number is required")
                .MaximumLength(20);

            RuleFor(x => x.EmployeeId)
                .GreaterThan(0).WithMessage("Employee is required");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Category is required");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(200);

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required");

            RuleFor(x => x.Priority)
                .IsInEnum().WithMessage("Invalid priority");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid status");

            RuleFor(x => x.CreatedOn)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("CreatedOn cannot be in future");

            RuleFor(x => x.UpdatedOn)
                .GreaterThanOrEqualTo(x => x.CreatedOn)
                .WithMessage("UpdatedOn must be after CreatedOn");

            RuleFor(x => x.ClosedOn)
                .GreaterThanOrEqualTo(x => x.CreatedOn)
                .When(x => x.ClosedOn.HasValue)
                .WithMessage("ClosedOn must be after CreatedOn");

            RuleFor(x => x.AssignedToId)
                .NotEqual(x => x.EmployeeId)
                .When(x => x.AssignedToId.HasValue)
                .WithMessage("Employee cannot assign request to themselves");
        }
    }
}

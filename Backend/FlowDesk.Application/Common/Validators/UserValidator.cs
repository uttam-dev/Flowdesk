using FlowDesk.Domain.Entities;
using FluentValidation;

namespace FlowDesk.Application.Common.Validators
{
    public class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .MaximumLength(150);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(150);

            RuleFor(x => x.PasswordHash)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .MaximumLength(256);

            RuleFor(x => x.RoleId)
                .GreaterThan(0).WithMessage("Valid role is required");

            RuleFor(x => x.ManagerId)
                .NotEqual(x => x.UserId)
                .WithMessage("User cannot be their own manager")
                .When(x => x.ManagerId.HasValue);

            RuleFor(x => x.CreatedOn)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("Created date cannot be in future");
        }
    }
}

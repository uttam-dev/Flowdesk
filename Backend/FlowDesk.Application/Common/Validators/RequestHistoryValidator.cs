using FlowDesk.Domain.Entities;
using FluentValidation;

namespace FlowDesk.Application.Common.Validators
{
    public class RequestHistoryValidator : AbstractValidator<RequestHistory>
    {
        public RequestHistoryValidator()
        {
            RuleFor(x => x.RequestId)
                .GreaterThan(0).WithMessage("RequestId is required");

            RuleFor(x => x.ChangedById)
                .GreaterThan(0).WithMessage("ChangedBy is required");

            RuleFor(x => x.OldStatus)
                .IsInEnum().WithMessage("OldStatus is invalid");

            RuleFor(x => x.NewStatus)
                .IsInEnum().WithMessage("NewStatus is invalid");

            RuleFor(x => x)
                .Must(x => x.OldStatus != x.NewStatus)
                .WithMessage("OldStatus and NewStatus cannot be the same");

            RuleFor(x => x.ChangedOn)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("ChangedOn cannot be in future");
        }
    }
}

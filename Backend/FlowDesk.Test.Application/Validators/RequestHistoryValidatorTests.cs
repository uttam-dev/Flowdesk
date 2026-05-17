using FlowDesk.Application.Common.Validators;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FluentValidation.TestHelper;

namespace FlowDesk.Application.Tests.Validators
{
    public class RequestHistoryValidatorTests
    {
        private readonly RequestHistoryValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Data()
        {
            var model = new RequestHistory
            {
                RequestId = 1,
                ChangedById = 1,
                OldStatus = RequestStatusEnum.Open,
                NewStatus = RequestStatusEnum.InProgress,
                ChangedOn = DateTime.UtcNow.AddSeconds(-1),

            };

            var result = _validator.TestValidate(model);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_RequestId_Invalid()
        {
            var model = new RequestHistory { RequestId = 0 };

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.RequestId);
        }

        [Fact]
        public void Should_Fail_When_ChangedById_Invalid()
        {
            var model = new RequestHistory { ChangedById = 0 };

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.ChangedById);
        }

        [Fact]
        public void Should_Fail_When_Status_Same()
        {
            var model = new RequestHistory
            {
                OldStatus = RequestStatusEnum.Open,
                NewStatus = RequestStatusEnum.Open
            };

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x);
        }

        [Fact]
        public void Should_Fail_When_ChangedOn_In_Future()
        {
            var model = new RequestHistory
            {
                ChangedOn = DateTime.UtcNow.AddMinutes(1)
            };

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.ChangedOn);
        }
    }
}

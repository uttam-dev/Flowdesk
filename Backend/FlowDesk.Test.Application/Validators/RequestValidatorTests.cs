using FlowDesk.Application.Common.Validators;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FluentValidation.TestHelper;

namespace FlowDesk.Application.Tests.Validators
{
    public class RequestValidatorTests
    {
        private readonly RequestValidator _validator = new();

        private Request GetValidModel()
        {
            return new Request
            {
                RequestNumber = "REQ-2026-000001",
                EmployeeId = 1,
                CategoryId = 1,
                Title = "Test Title",
                Description = "Test Description",
                Priority = PriorityEnum.Medium,
                Status = RequestStatusEnum.Open,
                CreatedOn = DateTime.UtcNow.AddSeconds(-10),
                UpdatedOn = DateTime.UtcNow,
                AssignedToId = 2
            };
        }

        [Fact]
        public void Should_Pass_For_Valid_Data()
        {
            var model = GetValidModel();

            var result = _validator.TestValidate(model);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_RequestNumber_Empty()
        {
            var model = GetValidModel();
            model.RequestNumber = "";

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.RequestNumber);
        }

        [Fact]
        public void Should_Fail_When_EmployeeId_Invalid()
        {
            var model = GetValidModel();
            model.EmployeeId = 0;

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.EmployeeId);
        }

        [Fact]
        public void Should_Fail_When_CategoryId_Invalid()
        {
            var model = GetValidModel();
            model.CategoryId = 0;

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.CategoryId);
        }

        [Fact]
        public void Should_Fail_When_Title_Empty()
        {
            var model = GetValidModel();
            model.Title = "";

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Should_Fail_When_Description_Empty()
        {
            var model = GetValidModel();
            model.Description = "";

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Should_Fail_When_CreatedOn_In_Future()
        {
            var model = GetValidModel();
            model.CreatedOn = DateTime.UtcNow.AddMinutes(1);

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.CreatedOn);
        }

        [Fact]
        public void Should_Fail_When_UpdatedOn_Before_CreatedOn()
        {
            var model = GetValidModel();
            model.UpdatedOn = model.CreatedOn.AddMinutes(-1);

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.UpdatedOn);
        }

        [Fact]
        public void Should_Fail_When_ClosedOn_Before_CreatedOn()
        {
            var model = GetValidModel();
            model.ClosedOn = model.CreatedOn.AddMinutes(-1);

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.ClosedOn);
        }

        [Fact]
        public void Should_Fail_When_AssignedTo_Same_As_Employee()
        {
            var model = GetValidModel();
            model.AssignedToId = model.EmployeeId;

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.AssignedToId);
        }
    }
}

using FlowDesk.Application.Common.Validators;
using FlowDesk.Domain.Entities;
using FluentValidation.TestHelper;

namespace FlowDesk.Application.Tests.Validators
{
    public class UserValidatorTests
    {
        private readonly UserValidator _validator = new();

        private User GetValidModel()
        {
            return new User
            {
                UserId = 1,
                FullName = "Uttam Prajapati",
                Email = "test@example.com",
                PasswordHash = "123456",
                RoleId = 1,
                ManagerId = 2,
                CreatedOn = DateTime.UtcNow.AddSeconds(-10)
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
        public void Should_Fail_When_FullName_Empty()
        {
            var model = GetValidModel();
            model.FullName = "";

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.FullName);
        }

        [Fact]
        public void Should_Fail_When_Email_Invalid()
        {
            var model = GetValidModel();
            model.Email = "invalid-email";

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Should_Fail_When_Password_Too_Short()
        {
            var model = GetValidModel();
            model.PasswordHash = "123";

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.PasswordHash);
        }

        [Fact]
        public void Should_Fail_When_RoleId_Invalid()
        {
            var model = GetValidModel();
            model.RoleId = 0;

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.RoleId);
        }

        [Fact]
        public void Should_Fail_When_User_Is_Their_Own_Manager()
        {
            var model = GetValidModel();
            model.ManagerId = model.UserId;

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.ManagerId);
        }

        [Fact]
        public void Should_Fail_When_CreatedOn_In_Future()
        {
            var model = GetValidModel();
            model.CreatedOn = DateTime.UtcNow.AddMinutes(1);

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.CreatedOn);
        }
    }
}

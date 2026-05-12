using FlowDesk.Application.Common.Validators;
using FlowDesk.Domain.Entities;
using FluentValidation.TestHelper;

namespace FlowDesk.Application.Tests.Validators
{
    public class RoleValidatorTests
    {
        private readonly RoleValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Data()
        {
            var model = new Role { RoleName = "Admin" };

            var result = _validator.TestValidate(model);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_RoleName_Empty()
        {
            var model = new Role { RoleName = "" };

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.RoleName);
        }

        [Fact]
        public void Should_Fail_When_RoleName_Whitespace()
        {
            var model = new Role { RoleName = "   " };

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.RoleName);
        }

        [Fact]
        public void Should_Fail_When_RoleName_Too_Long()
        {
            var model = new Role
            {
                RoleName = new string('a', 101)
            };

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.RoleName);
        }
    }
}

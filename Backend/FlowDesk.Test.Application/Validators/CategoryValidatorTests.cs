using FlowDesk.Application.Common.Validators;
using FlowDesk.Domain.Entities;
using FluentValidation.TestHelper;
using Xunit;

namespace FlowDesk.Application.Tests.Validators
{
    public class CategoryValidatorTests
    {
        private readonly CategoryValidator _validator = new();

        [Fact]
        public void Should_Have_Error_When_CategoryName_Is_Empty()
        {
            var model = new Category { CategoryName = "" };

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.CategoryName);
        }

        [Fact]
        public void Should_Have_Error_When_CategoryName_Exceeds_MaxLength()
        {
            var model = new Category
            {
                CategoryName = new string('A', 101)
            };

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.CategoryName);
        }

        [Fact]
        public void Should_Have_Error_When_CreatedOn_Is_In_Future()
        {
            var model = new Category
            {
                CategoryName = "IT",
                CreatedOn = DateTime.UtcNow.AddDays(1)
            };

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.CreatedOn);
        }

        [Fact]
        public void Should_Have_Error_When_UpdatedOn_Is_Before_CreatedOn()
        {
            var model = new Category
            {
                CategoryName = "IT",
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow.AddDays(-1)
            };

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.UpdatedOn);
        }

        [Fact]
        public void Should_Not_Have_Error_For_Valid_Category()
        {
            var now = DateTime.Now;
            var model = new Category
            {
                CategoryName = "IT",
                CreatedOn = now,
                UpdatedOn = now
            };

            var result = _validator.TestValidate(model);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}

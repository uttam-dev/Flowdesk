using FlowDesk.Application.Common.Validators;
using FlowDesk.Domain.Entities;
using FluentValidation.TestHelper;

namespace FlowDesk.Application.Tests.Validators
{
    public class CommentValidatorTests
    {
        private readonly CommentValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Data()
        {
            var model = new Comment
            {
                RequestId = 1,
                CommentById = 1,
                CommentText = "Valid comment",
                CreatedOn = DateTime.UtcNow.AddSeconds(-1)
            };

            var result = _validator.TestValidate(model);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_RequestId_Invalid()
        {
            var model = new Comment { RequestId = 0 };

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.RequestId);
        }

        [Fact]
        public void Should_Fail_When_CommentById_Invalid()
        {
            var model = new Comment { CommentById = 0 };

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.CommentById);
        }

        [Fact]
        public void Should_Fail_When_CommentText_Empty()
        {
            var model = new Comment { CommentText = "" };

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.CommentText);
        }

        [Fact]
        public void Should_Fail_When_CommentText_Too_Long()
        {
            var model = new Comment
            {
                CommentText = new string('a', 2001)
            };

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.CommentText);
        }

        [Fact]
        public void Should_Fail_When_CreatedOn_In_Future()
        {
            var model = new Comment
            {
                CreatedOn = DateTime.UtcNow.AddMinutes(1)
            };

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.CreatedOn);
        }
    }
}

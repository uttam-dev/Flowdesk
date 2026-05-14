using System.ComponentModel.DataAnnotations;

namespace FlowDesk.Application.Features.Users.DTOs
{
    public class UserResetPasswordDto
    {
        [Required(ErrorMessage = "password is required.")]
        [MinLength(6, ErrorMessage = "password must be at least 6 characters long.")]
        public string NewPassword { get; set; } = null!;
    }
}

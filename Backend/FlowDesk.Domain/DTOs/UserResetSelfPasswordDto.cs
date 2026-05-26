using System.ComponentModel.DataAnnotations;

namespace FlowDesk.Domain.DTOs
{
    public class UserResetSelfPasswordDto
    {
        [Required(ErrorMessage = "password is required.")]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "password is required.")]
        [MinLength(8, ErrorMessage = "password must be at least 8 characters long.")]
        public string NewPassword { get; set; } = null!;
    }
}

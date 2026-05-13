using System.ComponentModel.DataAnnotations;

namespace FlowDesk.Domain.DTOs
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Email is requried")]
        [EmailAddress(ErrorMessage = "Email is invalid")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }
}

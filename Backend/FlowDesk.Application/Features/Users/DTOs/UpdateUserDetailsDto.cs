using System.ComponentModel.DataAnnotations;

namespace FlowDesk.Application.Features.Users.DTOs
{
    public class UpdateUserDetailsDto
    {
        [Required(ErrorMessage = "Fullname is required.")]
        [StringLength(100)]
        [MinLength(3, ErrorMessage = "Fullname must be at least 3 characters long.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "RoleId is required.")]
        public int RoleId { get; set; }
        public int? ManagerId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

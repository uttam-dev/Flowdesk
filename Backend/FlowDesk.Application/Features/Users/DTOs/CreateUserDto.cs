using FlowDesk.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FlowDesk.Application.Features.Users.DTOs
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "Fullname is required")]
        [MaxLength(50,ErrorMessage = "Fullname must be in 50 charecters")]
        [MinLength(3,ErrorMessage = "Fullname must be minimum 3 charecters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "email is required")]
        [EmailAddress(ErrorMessage = "email is invlaid")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "password is requried")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role id is required")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Manager id is required")]
        public int? ManagerId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

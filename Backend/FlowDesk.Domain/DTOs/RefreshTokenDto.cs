using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FlowDesk.Domain.DTOs
{
    public class RefreshTokenDto
    {
        [Required(ErrorMessage = "Refresh token required")]
        public string? RefreshToken { get; set; }
    }
}

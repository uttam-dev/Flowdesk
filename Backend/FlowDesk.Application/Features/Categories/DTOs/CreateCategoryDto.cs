using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FlowDesk.Application.Features.Categories.DTOs
{
    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        [MinLength(1, ErrorMessage = "Category is required")]
        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "SLA Hourse is required.")]
        public int SLAHours { get; set; }

        [Required(ErrorMessage = "Approvel status is required")]
        public bool IsApprovalRequired { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FlowDesk.Application.Features.Categories.DTOs
{
    public class UpdateCategoryDto
    {
        [Required(ErrorMessage = "Category name is required")]
        public string? CategoryName { get; set; }

        public int SLAHours { get; set; }

        [Required(ErrorMessage = "Approve status is requried")]
        public bool? IsApprovalRequired { get; set; }
    }
}

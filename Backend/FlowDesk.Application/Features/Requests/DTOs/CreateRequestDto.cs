using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FlowDesk.Application.Features.Requests.DTOs
{
    public class CreateRequestDto
    {
        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }
        
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Priority is required")]
        public PriorityEnum Priority { get; set; }
    }
}

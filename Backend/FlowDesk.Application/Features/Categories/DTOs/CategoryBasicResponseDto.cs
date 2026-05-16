using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Categories.DTOs
{
    public class CategoryBasicResponseDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}

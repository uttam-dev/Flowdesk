using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FlowDesk.Application.Features.Requests.DTOs
{
    public class AssignRequestDto
    {
        [Required(ErrorMessage = "Assign id is required")]
        public int AssignToId { get; set; }
        public int RemarksId { get; set; }
    }
}
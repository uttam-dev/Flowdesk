using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FlowDesk.Application.Features.Requests.DTOs
{
    public class AddRequestCommentDto
    {
        [Required(ErrorMessage = "Comment is required.")]
        public string? CommentText { get; set; }
    }
}

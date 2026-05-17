using FlowDesk.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Requests.DTOs
{
    public class CommentResponseDto
    {
        public int CommentId { get; set; }
        public int RequestId { get; set; }
        public string? CommentByName { get; set; }
        public string? CommentByRoleName { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public bool IsCurrentUser { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}

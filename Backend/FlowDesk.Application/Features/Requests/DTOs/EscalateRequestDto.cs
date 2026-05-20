using System.ComponentModel.DataAnnotations;

namespace FlowDesk.Application.Features.Requests.DTOs
{
    public class EscalateRequestDto
    {
        [Required]
        public string EscalationReason { get; set; } = string.Empty;
    }
}

using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;

namespace FlowDesk.Application.Features.Requests.DTOs
{
    public class UpdateRequestStatusDto
    {
        public RequestStatusEnum Status { get; set; }
        public int RemarksId { get; set; }
    }
}

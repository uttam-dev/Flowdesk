using FlowDesk.Domain.Enums;

namespace FlowDesk.Domain.Entities
{
    public class Request
    {
        public int RequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;

        public int EmployeeId { get; set; }
        public User Employee { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PriorityEnum Priority { get; set; }   
        public RequestStatusEnum Status { get; set; } = RequestStatusEnum.Open;
        
        public int? AssignedToId { get; set; }
        public User? AssignedUser { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime? ClosedOn { get; set; }
    }
}

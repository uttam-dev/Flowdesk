namespace FlowDesk.Application.Features.Categories.DTOs
{
    public class CategoryResponseDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsApprovalRequired { get; set; }
        public bool IsActive { get; set; } = true;
        public int SLAHours { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}

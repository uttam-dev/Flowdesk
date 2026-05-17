namespace FlowDesk.Domain.DTOs
{
    public class FilterRequestQueryDto
    {
        public int PageSize { get; set; } = 5;
        public int PageNumber { get; set; } = 1;
        public int Status { get; set; }
        public int CategoryId { get; set; }
    }
}

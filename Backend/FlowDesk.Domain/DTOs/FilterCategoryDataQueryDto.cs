namespace FlowDesk.Domain.DTOs
{
    public class FilterCategoryDataQueryDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public bool? IsActive { get; set; }
    }
}

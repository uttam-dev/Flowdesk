namespace FlowDesk.Domain.DTOs
{
    public class FilterUserDataQueryDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
    }
}

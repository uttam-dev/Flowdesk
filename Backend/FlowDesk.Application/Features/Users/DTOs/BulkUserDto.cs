namespace FlowDesk.Application.Features.Users.DTOs
{
    public class BulkUserDto
    {
        public int RowNumber { get; set; }

        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;

        public string RoleName { get; set; } = default!;
        public string? ManagerEmail { get; set; }

        public bool? IsActive { get; set; }
    }
}

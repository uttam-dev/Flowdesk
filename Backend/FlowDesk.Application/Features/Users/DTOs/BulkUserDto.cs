namespace FlowDesk.Application.Features.Users.DTOs
{
    public class BulkUserDto
    {

        public int RowNumber { get; set; }
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public int RoleId { get; set; }
        public int? ManagerId { get; set; }
        public bool? IsActive { get; set; }
    }
}

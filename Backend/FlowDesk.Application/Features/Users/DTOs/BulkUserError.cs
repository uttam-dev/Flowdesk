namespace FlowDesk.Application.Features.Users.DTOs
{
    public class BulkUserError
    {
        public int RowNumber { get; set; }
        public string Email { get; set; } = "";
        public string Error { get; set; } = "";
    }

}

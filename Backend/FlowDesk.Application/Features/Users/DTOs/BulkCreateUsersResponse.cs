namespace FlowDesk.Application.Features.Users.DTOs
{
    public class BulkCreateUsersResponse
    {
        public int Total { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<BulkUserError> Errors { get; set; } = new();
    }
}

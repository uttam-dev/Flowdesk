namespace FlowDesk.Domain.Entities
{
    public class Comment
    {
        public int CommentId { get; set; }

        public int RequestId { get; set; }
        public Request? Request { get; set; }

        public int CommentById { get; set; }
        public User? Commenter { get; set; }

        public string CommentText { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
    }
}

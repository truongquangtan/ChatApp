namespace ChatApp.Models
{
    public class EndConversationRequest
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string RequestUserId { get; set; }
        public string ConfirmUserId { get; set; }
        public string GroupId { get; set; }
        public bool IsConfimed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

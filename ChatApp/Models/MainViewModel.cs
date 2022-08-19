namespace ChatApp.Models
{
    public class MainViewModel
    {
        public string AuthorUserId { get; set; }
        public List<ChatViewModel> ChatViewModels { get; set; }
        public string? ShowChatMessageForUserId { get; set; }
    }
}

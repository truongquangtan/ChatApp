namespace ChatApp.Models
{
    public class FirstMessageModel
    {
        public string AuthorRole { get; set; }
        public string AuthorId { get; set; }
        public List<ChatViewModel> GroupsMessage { get; set; }
    }
}

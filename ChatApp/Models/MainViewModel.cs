namespace ChatApp.Models
{
    public class MainViewModel
    {
        public string GroupId { get; set; }
        public bool IsGroupActive { get; set; }
        public string AuthorUserId { get; set; }
        public string AuthorRole { get; set; }
        public string RecipientName { get; set; }
        public string RecipientId { get; set; }
        public List<Message> MyMessages { get; set; }
        public List<Message> OtherMessages { get; set; }
    }
}

namespace ChatApp.Models
{
    public class ContactRequest
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public virtual User User { get; set; }
    }
}

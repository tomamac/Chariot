namespace Chariot.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public DateTime SentAt { get; set; }
        public int? UserId { get; set; }
        public int ChatroomId { get; set; }
        public bool IsSystem { get; set; } = false;

        public User User { get; set; } = null!;
        public Chatroom Chatroom { get; set; } = null!;
    }
}

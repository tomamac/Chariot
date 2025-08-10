namespace Chariot.Entities
{
    public class ChatroomUser
    {
        public int UserId { get; set; }
        public int ChatroomId { get; set; }

        public User User { get; set; } = null!;
        public Chatroom Chatroom { get; set; } = null!;

        public DateTime JoinedAt { get; set; }
        public bool IsAdmin { get; set; }
    }
}

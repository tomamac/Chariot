namespace Chariot.Entities
{
    public class Chatroom
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public List<ChatroomUser> ChatroomUsers { get; } = [];
        public List<Message> Messages { get; } = [];
    }
}

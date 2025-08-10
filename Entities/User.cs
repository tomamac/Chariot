namespace Chariot.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string HashedPassword { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool IsOnline { get; set; }
        public List<ChatroomUser> ChatroomUsers { get; } = [];
    }
}

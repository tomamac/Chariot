namespace Chariot.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string? HashedPassword { get; set; }
        public string DisplayName { get; set; } = null!;
        public bool IsOnline { get; set; }
        public string Role { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExp { get; set; }
        public List<ChatroomUser> ChatroomUsers { get; } = [];
        public List<Message> Messages { get; } = [];
    }
}

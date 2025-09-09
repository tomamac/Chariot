namespace Chariot.Models
{
    public class MessageResponseDTO
    {
        public int RoomId { get; set; }
        public int UserId {  get; set; }
        public string Displayname { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}

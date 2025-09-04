using Chariot.Entities;

namespace Chariot.Models
{
    public class ChatroomInfoDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public DateTime? LastActivityAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public MessageResponseDTO? LastMessage { get; set; }
    }
}

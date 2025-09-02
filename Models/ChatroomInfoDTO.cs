using Chariot.Entities;

namespace Chariot.Models
{
    public class ChatroomInfoDTO
    {
        public Chatroom Chatroom { get; set; } = new();
        public Message? LastMessage { get; set; }
    }
}

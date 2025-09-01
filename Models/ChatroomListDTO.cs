using Chariot.Entities;

namespace Chariot.Models
{
    public class ChatroomListDTO
    {
        public Chatroom Chatroom { get; set; } = new();
        public Message? LastMessage { get; set; }

    }
}

using Chariot.Data;
using Chariot.Entities;
using Chariot.Models;

namespace Chariot.Services
{
    public class ChatService(ChariotDbContext context) : IChatService
    {
        public Task<int?> CreateChatAsync(int userId, string chatName)
        {
            throw new NotImplementedException();
        }

        public Task<List<Chatroom>?> GetChatroomsAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<List<UserInfoDTO>?> GetChatroomUsersAsync(int roomId)
        {
            throw new NotImplementedException();
        }

        public Task<List<MessageResponseDTO>?> GetMessageHistoryAsync(int roomId)
        {
            throw new NotImplementedException();
        }

        public Task<int?> JoinChatAsync(int userId, string roomCode)
        {
            throw new NotImplementedException();
        }

        public Task<string?> LeaveChatAsync(int userId, int roomId)
        {
            throw new NotImplementedException();
        }

        public Task<MessageResponseDTO?> SaveMessageAsync(int userId, int roomId, string content)
        {
            throw new NotImplementedException();
        }
    }
}

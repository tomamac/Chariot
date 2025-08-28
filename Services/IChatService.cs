using Chariot.Entities;
using Chariot.Models;

namespace Chariot.Services
{
    public interface IChatService
    {
        //Create chat (userid, chatname) return room id
        Task<int?> CreateChatAsync(int userId, string chatName);
        Task<int?> JoinChatAsync(int userId, string roomCode);
        Task<string?> LeaveChatAsync(int userId, int roomId);
        Task<MessageResponseDTO?> SaveMessageAsync(int userId, int roomId, string content);
        //TODO: Pagination query
        Task<List<Chatroom>?> GetChatroomsAsync(int userId);
        Task<List<MessageResponseDTO>?> GetMessageHistoryAsync(int roomId);
        Task<List<UserInfoDTO>?> GetChatroomUsersAsync(int roomId);
    }
}

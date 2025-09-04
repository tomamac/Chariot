using Chariot.Entities;
using Chariot.Models;

namespace Chariot.Services
{
    public interface IChatService
    {
        Task<ChatroomInfoDTO?> CreateChatAsync(int userId, string chatName);
        Task<(JoinChatResult Result, ChatroomInfoDTO? Chatroom)> JoinChatAsync(int userId, string roomCode);
        Task<string?> LeaveChatAsync(int userId, int roomId);
        Task<MessageResponseDTO?> SaveMessageAsync(int userId, int roomId, string content);
        Task<MessageResponseDTO?> SaveSystemMessageAsync(int roomId, string content);
        Task SetOnlineStatusAsync(int userId, bool isOnline);
        Task<bool> IsUserInChatroomAsync(int userId, int roomId);
        Task<List<ChatroomInfoDTO>> GetChatroomsAsync(int userId);
        Task<List<MessageResponseDTO>> GetMessageHistoryAsync(int roomId);
        Task<List<UserInfoDTO>> GetChatroomUsersAsync(int roomId);
    }
}

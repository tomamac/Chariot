using Chariot.Data;
using Chariot.Entities;
using Chariot.Models;
using Microsoft.EntityFrameworkCore;

namespace Chariot.Services
{
    public class ChatService(ChariotDbContext context) : IChatService
    {
        public async Task<int?> CreateChatAsync(int userId, string chatName)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null)
            {
                return null;
            }

            var newChat = new Chatroom
            {
                Name = chatName,
                Code = Guid.NewGuid().ToString("N").Substring(0, 8),
                CreatedAt = DateTime.UtcNow,
            };

            context.Chatrooms.Add(newChat);

            var chatAdmin = new ChatroomUser
            {
                UserId = userId,
                ChatroomId = newChat.Id,
                JoinedAt = newChat.CreatedAt,
                IsAdmin = true,
            };

            context.ChatroomsUser.Add(chatAdmin);
            await context.SaveChangesAsync();

            return newChat.Id;
        }

        public async Task<int?> JoinChatAsync(int userId, string roomCode)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var chatroom = await context.Chatrooms.FirstOrDefaultAsync(c => c.Code == roomCode);
            if (user is null || chatroom is null)
            {
                return null;
            }

            if (await context.ChatroomsUser.AnyAsync(cu => cu.UserId == userId && cu.Chatroom.Code == roomCode))
            {
                return null;
            }

            var chatUser = new ChatroomUser
            {
                UserId = userId,
                ChatroomId = chatroom.Id,
                JoinedAt = DateTime.UtcNow,
                IsAdmin = false,
            };

            context.ChatroomsUser.Add(chatUser);
            chatroom.LastActivityAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return chatroom.Id;
        }

        public async Task<string?> LeaveChatAsync(int userId, int roomId)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var chatroom = await context.Chatrooms.FirstOrDefaultAsync(c => c.Id == roomId);
            if (user is null || chatroom is null)
            {
                return null;
            }

            var leaver = await context.ChatroomsUser.FirstOrDefaultAsync(cu => cu.UserId == userId && cu.ChatroomId == roomId);
            if (leaver is null)
            {
                return null;
            }

            context.ChatroomsUser.Remove(leaver);
            chatroom.LastActivityAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return user.DisplayName;
        }

        public async Task<MessageResponseDTO?> SaveMessageAsync(int userId, int roomId, string content)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var chatroom = await context.Chatrooms.FirstOrDefaultAsync(c => c.Id == roomId);
            if (user is null || chatroom is null)
            {
                return null;
            }

            var message = new Message
            {
                Content = content,
                SentAt = DateTime.UtcNow,
                UserId = userId,
                ChatroomId = roomId,
            };

            context.Messages.Add(message);
            chatroom.LastActivityAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return new MessageResponseDTO
            {
                UserId = userId,
                Displayname = user.DisplayName,
                Content = content,
                SentAt = message.SentAt
            };
        }

        public async Task<List<ChatroomListDTO>?> GetChatroomsAsync(int userId)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null)
            {
                return null;
            }

            return await context.ChatroomsUser
                .Where(cu => cu.UserId == userId)
                .Select(cu => cu.Chatroom)
                .Select(c => new ChatroomListDTO
                {
                    Chatroom = c,
                    LastMessage = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault()
                })
                .OrderByDescending(c => c.Chatroom.LastActivityAt ?? c.Chatroom.CreatedAt)
                .ToListAsync();
        }
        //list<userinfo> (user in chatroom)
        public Task<List<UserInfoDTO>?> GetChatroomUsersAsync(int roomId)
        {
            throw new NotImplementedException();
        }
        //list<message> message history
        public Task<List<MessageResponseDTO>?> GetMessageHistoryAsync(int roomId)
        {
            throw new NotImplementedException();
        }
    }
}

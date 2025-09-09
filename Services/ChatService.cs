using Chariot.Data;
using Chariot.Entities;
using Chariot.Models;
using Microsoft.EntityFrameworkCore;

namespace Chariot.Services
{
    public enum JoinChatResult
    {
        Success,
        NotFound,
        AlreadyJoined
    }

    public enum DeleteChatResult
    {
        Success,
        NotFound,
        Forbidden
    }

    public class ChatService(ChariotDbContext context) : IChatService
    {
        public async Task<ChatroomInfoDTO?> CreateChatAsync(int userId, string chatName)
        {
            if (!await context.Users.AnyAsync(u => u.Id == userId))
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
                Chatroom = newChat,
                JoinedAt = newChat.CreatedAt,
                IsAdmin = true,
            };

            context.ChatroomsUser.Add(chatAdmin);
            await context.SaveChangesAsync();

            return new ChatroomInfoDTO
            {
                Id = newChat.Id,
                Name = newChat.Name,
                Code = newChat.Code,
                CreatedAt = newChat.CreatedAt,
                LastMessage = null
            };
        }

        public async Task<DeleteChatResult> DeleteChatAsync(int userId, int roomId) 
        {
            var chat = await context.Chatrooms.FindAsync(roomId);
            if (chat is null)
                return DeleteChatResult.NotFound;

            var admin = await context.ChatroomsUser
                .FirstOrDefaultAsync(cu => cu.ChatroomId == roomId && cu.UserId == userId);
            if (admin is null || !admin.IsAdmin)
                return DeleteChatResult.Forbidden;

            context.Chatrooms.Remove(chat);
            await context.SaveChangesAsync();
            return DeleteChatResult.Success;
        }

        public async Task<(JoinChatResult Result, ChatroomInfoDTO? Chatroom)> JoinChatAsync(int userId, string roomCode)
        {
            var chat = await context.Chatrooms.FirstOrDefaultAsync(c => c.Code == roomCode);
            if (!await context.Users.AnyAsync(u => u.Id == userId) || chat is null)
            {
                return (JoinChatResult.NotFound, null);
            }

            if (await IsUserInChatroomAsync(userId,chat.Id))
            {
                return (JoinChatResult.AlreadyJoined, null);
            }

            var chatUser = new ChatroomUser
            {
                UserId = userId,
                ChatroomId = chat.Id,
                JoinedAt = DateTime.UtcNow,
                IsAdmin = false,
            };

            context.ChatroomsUser.Add(chatUser);
            chat.LastActivityAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return (JoinChatResult.Success, await context.Chatrooms
                .Where(c => c.Id == chat.Id)
                .Select(c => new ChatroomInfoDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Code = c.Code,
                    LastActivityAt = c.LastActivityAt,
                    CreatedAt = c.CreatedAt,
                    LastMessage = c.Messages.OrderByDescending(m => m.SentAt)
                    .Select(m => new MessageResponseDTO
                    {
                        UserId = m.UserId ?? 0,
                        Content = m.Content,
                        Displayname = m.IsSystem ? "System" : m.User.DisplayName,
                        SentAt = m.SentAt,
                    })
                    .FirstOrDefault()
                })
                .FirstOrDefaultAsync());
        }

        public async Task<string?> LeaveChatAsync(int userId, int roomId)
        {
            if (!await context.Users.AnyAsync(u => u.Id == userId))
            {
                return null;
            }

            var leaver = await context.ChatroomsUser
                .Include(cu => cu.User)
                .Include(cu => cu.Chatroom)
                .FirstOrDefaultAsync(cu => cu.UserId == userId && cu.ChatroomId == roomId);
            if (leaver is null)
            {
                return null;
            }

            string leaverName = leaver.User.DisplayName;
            leaver.Chatroom.LastActivityAt = DateTime.UtcNow;
            context.ChatroomsUser.Remove(leaver);
            await context.SaveChangesAsync();

            return leaverName;
        }
        public async Task<MessageResponseDTO?> SaveMessageAsync(int userId, int roomId, string content)
        {
            var senderName = await context.Users.Where(u => u.Id == userId).Select(u => u.DisplayName).FirstOrDefaultAsync();
            var chat = await context.Chatrooms.FindAsync(roomId);
            if (senderName is null || chat is null || !await IsUserInChatroomAsync(userId, roomId))
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
            chat.LastActivityAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return new MessageResponseDTO
            {
                RoomId = roomId,
                UserId = userId,
                Displayname = senderName,
                Content = content,
                SentAt = message.SentAt
            };
        }

        public async Task<MessageResponseDTO?> SaveSystemMessageAsync(int roomId, string content)
        {
            var chat = await context.Chatrooms.FindAsync(roomId);
            if (chat is null)
            {
                return null;
            }

            var message = new Message
            {
                Content = content,
                SentAt = DateTime.UtcNow,
                UserId = null,
                ChatroomId = roomId,
                IsSystem = true
            };

            context.Messages.Add(message);
            chat.LastActivityAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return new MessageResponseDTO
            {
                RoomId = roomId,
                UserId = 0,
                Displayname = "System",
                Content = content,
                SentAt = message.SentAt
            };
        }

        public async Task SetOnlineStatusAsync(int userId, bool isOnline)
        {
            var user = await context.Users.FindAsync(userId);
            if (user is null) return;
            user.IsOnline = isOnline;
            await context.SaveChangesAsync();
        }

        public async Task<bool> IsUserInChatroomAsync(int userId, int roomId)
        {
            return await context.ChatroomsUser.AnyAsync(cu => cu.UserId == userId && cu.ChatroomId == roomId);
        }
        //TODO: Limit query for get services
        public async Task<List<ChatroomInfoDTO>> GetChatroomsAsync(int userId)
        {
            if (!await context.Users.AnyAsync(u => u.Id == userId))
            {
                return [];
            }

            return await context.ChatroomsUser
                .Where(cu => cu.UserId == userId)
                .Select(cu => cu.Chatroom)
                .Select(c => new ChatroomInfoDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Code = c.Code,
                    LastActivityAt = c.LastActivityAt,
                    CreatedAt = c.CreatedAt,
                    LastMessage = c.Messages.OrderByDescending(m => m.SentAt)
                    .Select(m => new MessageResponseDTO
                    {
                        UserId = m.UserId ?? 0,
                        Content = m.Content,
                        Displayname = m.IsSystem ? "System" : m.User.DisplayName,
                        SentAt = m.SentAt,
                    })
                    .FirstOrDefault()
                })
                .OrderByDescending(c => c.LastActivityAt ?? c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<UserInfoDTO>> GetChatroomUsersAsync(int roomId)
        {
            if (!await context.Chatrooms.AnyAsync(c => c.Id == roomId))
            {
                return [];
            }

            return await context.ChatroomsUser
                .Where(cu => cu.ChatroomId == roomId)
                .Select(cu => cu.User)
                .Select(u => new UserInfoDTO
                {
                    UserId = u.Id,
                    DisplayName = u.DisplayName,
                    IsOnline = u.IsOnline,
                })
                .ToListAsync();
        }

        public async Task<List<MessageResponseDTO>> GetMessageHistoryAsync(int roomId)
        {
            if (!await context.Chatrooms.AnyAsync(c => c.Id == roomId))
            {
                return [];
            }

            return await context.Messages
                .Where(m => m.ChatroomId == roomId)
                .Select(m => new MessageResponseDTO
                {
                    RoomId = roomId,
                    UserId = m.UserId ?? 0,
                    Displayname = m.IsSystem ? "System" : m.User.DisplayName,
                    Content = m.Content,
                    SentAt = m.SentAt,
                })
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }
    }
}

using Chariot.Models;
using Chariot.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;

namespace Chariot.Hubs
{
    public class ChatHub(IChatService chatService) : Hub
    {
        public async Task CreateRoom(string roomName)
        {
            if (int.TryParse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
            {
                var chatroom = await chatService.CreateChatAsync(userId, roomName);
                if (chatroom is null)
                {
                    throw new HubException("Failed to create chat");
                }
                await Groups.AddToGroupAsync(Context.ConnectionId, chatroom.Chatroom.Id.ToString()!);
                await Clients.Caller.SendAsync("RoomCreated", chatroom);
            }
            else await Clients.Caller.SendAsync("Error", new { message = "Invalid user ID claim" });
        }

        public async Task JoinRoom(string roomCode)
        {
            if (int.TryParse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
            {
                var roomId = await chatService.JoinChatAsync(userId, roomCode);
                if (roomId is null)
                {
                    await Clients.Caller.SendAsync("Error", new { message = "Room not found" });
                    return;
                }
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString()!);
            }
            else await Clients.Caller.SendAsync("Error", new { message = "Invalid user ID claim" });
        }

        public async Task SendMessage(int roomId, string message)
        {
            if (int.TryParse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
            {
                var messageRes = await chatService.SaveMessageAsync(userId, roomId, message);
                if (messageRes is null)
                {
                    await Clients.Caller.SendAsync("Error", new { message = "Failed to send message" });
                    return;
                }
                await Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", messageRes);
            }
            else await Clients.Caller.SendAsync("Error", new { message = "Invalid user ID claim" });
        }

        public async Task LeaveRoom(int roomId)
        {
            if (int.TryParse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
            {
                var leaver = await chatService.LeaveChatAsync(userId, roomId);
                if (leaver is null)
                {
                    await Clients.Caller.SendAsync("Error", new { message = "Failed to leave chat" });
                    return;
                }
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
                await Clients.Group(roomId.ToString()).SendAsync("ChatEvent", new { message = $"{leaver} has left the chat"});
            }
            else await Clients.Caller.SendAsync("Error", new { message = "Invalid user ID claim" });
        }
    }
}

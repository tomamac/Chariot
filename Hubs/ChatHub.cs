using Chariot.Services;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Chariot.Hubs
{
    public class ChatHub(IChatService chatService) : Hub
    {
        public async Task CreateRoom(string roomName)
        {
            if (int.TryParse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
            {
                var roomId = await chatService.CreateChatAsync(userId, roomName);
                if (roomId is null)
                {
                    await Clients.Caller.SendAsync("Error", new { message = "Failed to create chat" });
                    return;
                }
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString()!);
            }
            else await Clients.Caller.SendAsync("Error", new { message = "Invalid user ID claim" });
        }

        public async Task JoinRoom(string roomCode)
        {

            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
        }

        public async Task SendMessage(string roomCode, string message)
        {
            var user = Context.User?.Identity?.Name; // from JWT claims
            await Clients.Group(roomCode).SendAsync("ReceiveMessage", user, message, DateTime.UtcNow);
        }

        public async Task LeaveRoom(string roomCode)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);
        }
    }
}

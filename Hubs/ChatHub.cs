using Chariot.Entities;
using Chariot.Services;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Chariot.Hubs
{
    public class ChatHub(IChatService chatService) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            var userId = GetUserId() ?? throw new HubException("Invalid user ID claim");
            await chatService.SetOnlineStatusAsync(userId, true);

            var chatroomList = await chatService.GetChatroomsAsync(userId);
            foreach (var chatroom in chatroomList)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, chatroom.Id.ToString()!);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (userId is not null)
            {
                await chatService.SetOnlineStatusAsync((int)userId, false);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task CreateRoom(string roomName)
        {
            var userId = GetUserId() ?? throw new HubException("Invalid user ID claim");
            var chatroom = await chatService.CreateChatAsync(userId, roomName) ?? throw new HubException("Failed to create chat");

            await Groups.AddToGroupAsync(Context.ConnectionId, chatroom.Id.ToString()!);
            await Clients.Caller.SendAsync("RoomCreated", chatroom);
        }

        public async Task DeleteRoom(int roomId)
        {
            var userId = GetUserId() ?? throw new HubException("Invalid user ID claim");
            DeleteChatResult result = await chatService.DeleteChatAsync(userId, roomId);
            switch (result)
            {
                case DeleteChatResult.NotFound:
                    throw new HubException("Room not found");
                case DeleteChatResult.Forbidden:
                    throw new HubException("Not allowed");
                default:
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
                    await Clients.Group(roomId.ToString()).SendAsync("RoomDeleted", roomId);
                    break;
            }
        }

        public async Task DisconnectFromDeletedRoom(int roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
        }

        public async Task JoinRoom(string roomCode)
        {
            var userId = GetUserId() ?? throw new HubException("Invalid user ID claim");
            var (result, chatroom) = await chatService.JoinChatAsync(userId, roomCode);

            switch (result)
            {
                case (JoinChatResult.NotFound):
                    throw new HubException("Room not found");
                case (JoinChatResult.AlreadyJoined):
                    throw new HubException("Already joined");
                default:
                    await Groups.AddToGroupAsync(Context.ConnectionId, chatroom!.Id.ToString());
                    await Clients.Caller.SendAsync("RoomJoined", chatroom);

                    var sysMessage = await chatService.SaveSystemMessageAsync(chatroom.Id,
                        $"{Context.User?.FindFirst(ClaimTypes.Name)?.Value!} has joined");
                    if (sysMessage is not null)
                        await Clients.Group(chatroom.Id.ToString()).SendAsync("ReceiveMessage", sysMessage);

                    break;
            }
        }

        public async Task SendMessage(int roomId, string message)
        {
            var userId = GetUserId() ?? throw new HubException("Invalid user ID claim");
            var messageRes = await chatService.SaveMessageAsync(userId, roomId, message) ?? throw new HubException("Failed to send message");
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", messageRes);
        }

        public async Task LeaveRoom(int roomId)
        {
            var userId = GetUserId() ?? throw new HubException("Invalid user ID claim");
            var leaver = await chatService.LeaveChatAsync(userId, roomId) ?? throw new HubException("Failed to leave chat");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());

            var sysMessage = await chatService.SaveSystemMessageAsync(roomId,
                        $"{Context.User?.FindFirst(ClaimTypes.Name)?.Value!} has left");
            if (sysMessage is not null)
                await Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", sysMessage);
        }

        private int? GetUserId()
        {
            return int.TryParse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId) ? userId : null;
        }
    }
}

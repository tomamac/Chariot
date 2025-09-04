using Chariot.Models;
using Chariot.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Chariot.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController(IChatService chatService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<ChatroomInfoDTO>>> GetChatrooms()
        {
            if (!int.TryParse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId)) return Unauthorized();

            var res = await chatService.GetChatroomsAsync(userId);
            return Ok(res);
        }

        [HttpGet("{roomId}/messages")]
        public async Task<ActionResult<List<MessageResponseDTO>>> GetChatHistory(int roomId)
        {
            if (!int.TryParse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId)) return Unauthorized();
            if (!await chatService.IsUserInChatroomAsync(userId, roomId)) return Forbid();

            var res = await chatService.GetMessageHistoryAsync(roomId);
            return Ok(res);
        }

        [HttpGet("{roomId}/users")]
        public async Task<ActionResult<List<UserInfoDTO>>> GetUsersInChatroom(int roomId)
        {
            if (!int.TryParse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId)) return Unauthorized();
            if (!await chatService.IsUserInChatroomAsync(userId, roomId)) return Forbid();

            var res = await chatService.GetChatroomUsersAsync(roomId);
            return Ok(res);
        }
    }
}

using Chariot.Entities;
using Chariot.Models;

namespace Chariot.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserAuthDTO req);
        Task<TokenResponseDTO?> LoginAsync(UserAuthDTO req);
        Task<TokenResponseDTO?> RefreshTokenAsync(RefreshTokenRequestDTO req);
        Task<TokenResponseDTO?> LoginGuestAsync(string guestName);
        Task<bool> LogoutAsync(int userId);
        Task<MyInfoDTO?> FetchUserInfoAsync(int userId);
    }
}

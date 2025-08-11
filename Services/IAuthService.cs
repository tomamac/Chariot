using Chariot.Entities;
using Chariot.Models;

namespace Chariot.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDTO req);
        Task<TokenResponseDTO?> LoginAsync(UserDTO req);
        Task<TokenResponseDTO?> RefreshTokenAsync(RefreshTokenRequestDTO req);
        Task<TokenResponseDTO?> LoginGuestAsync(GuestDTO req);
    }
}

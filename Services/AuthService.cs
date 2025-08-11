using Chariot.Data;
using Chariot.Entities;
using Chariot.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Chariot.Services
{
    public class AuthService(ChariotDbContext context, IConfiguration configuration) : IAuthService
    {
        public async Task<TokenResponseDTO?> LoginAsync(UserDTO req)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
            if (user is null)
            {
                return null;
            }

            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.HashedPassword!, req.Password)
                == PasswordVerificationResult.Failed)
            {
                return null;
            }

            return await CreateTokenResponse(user);
        }

        private async Task<TokenResponseDTO> CreateTokenResponse(User user)
        {
            return new TokenResponseDTO
            {
                AccessToken = CreateToken(user),
                RefreshToken = user.Role == "Guest" ? user.Role : await GenerateAndSaveRefreshTokenAsync(user)
            };
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExp = user.Role == "User" ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddDays(1);
            await context.SaveChangesAsync();
            return refreshToken;
        }

        private string GenerateRefreshToken()
        {
            var randomNum = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            return Convert.ToBase64String(randomNum);
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
            };


            //"jwt secret"
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["JWT_SECRET"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration["ISSUER"],
                audience: configuration["AUDIENCE"],
                claims: claims,
                expires: user.Role == "Guest" ? DateTime.UtcNow.AddDays(1) : DateTime.UtcNow.AddMinutes(5),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        public Task<TokenResponseDTO?> LoginGuestAsync(GuestDTO req)
        {
            throw new NotImplementedException();
        }

        public async Task<TokenResponseDTO?> RefreshTokenAsync(RefreshTokenRequestDTO req)
        {
            var user = await ValidateRefreshTokenAsync(req.UserId, req.RefreshToken);
            if (user is null) return null;
            return await CreateTokenResponse(user);
        }

        private async Task<User?> ValidateRefreshTokenAsync(int userId, string refreshToken)
        {
            var user = await context.Users.FindAsync(userId);
            if (user is null || user.RefreshToken != refreshToken
                || user.RefreshTokenExp <= DateTime.UtcNow)
            {
                return null;
            }

            return user;
        }

        public async Task<User?> RegisterAsync(UserDTO req)
        {
            if (await context.Users.AnyAsync(u => u.Username == req.Username))
            {
                return null;
            }

            var user = new User();
            var hashedPassword = new PasswordHasher<User>()
                .HashPassword(user, req.Password);

            user.Username = req.Username;
            user.HashedPassword = hashedPassword;

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return user;
        }
    }
}

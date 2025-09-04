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
    public class AuthService(ChariotDbContext context, IConfiguration configuration, IPasswordHasher<User> hasher) : IAuthService
    {
        public async Task<TokenResponseDTO?> LoginAsync(UserAuthDTO req)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
            if (user is null)
            {
                return null;
            }

            if (hasher.VerifyHashedPassword(user, user.HashedPassword!, req.Password)
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
                RefreshToken = user.Role == "Guest" ? null : await GenerateAndSaveRefreshTokenAsync(user)
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
            rng.GetBytes(randomNum);
            return Convert.ToBase64String(randomNum);
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("username", user.Username),
                new Claim(ClaimTypes.Name, user.DisplayName),
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

        public async Task<TokenResponseDTO?> LoginGuestAsync(string guestName)
        {

            var user = new User();

            user.Username = $"guest_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            user.DisplayName = guestName;
            user.Role = "Guest";
            user.CreatedAt = DateTime.UtcNow;

            while (await context.Users.AnyAsync(u => u.Username == user.Username))
            {
                user.Username = $"guest_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            }

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return await CreateTokenResponse(user);
        }

        public async Task<TokenResponseDTO?> RefreshTokenAsync(RefreshTokenRequestDTO req)
        {
            var principal = GetPrincipalFromExpiredToken(req.AccessToken);
            if (principal is null) return null;

            var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var user = await ValidateRefreshTokenAsync(userId, req.RefreshToken);
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

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = configuration["ISSUER"],
                ValidateAudience = true,
                ValidAudience = configuration["AUDIENCE"],
                ValidateLifetime = false,
                IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["JWT_SECRET"]!)),
                ValidateIssuerSigningKey = true,
            };

            ClaimsPrincipal? principal;
            try
            {
                principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
            }
            catch
            {
                return null;
            }

            return principal;
        }

        public async Task<User?> RegisterAsync(UserAuthDTO req)
        {
            if (await context.Users.AnyAsync(u => u.Username == req.Username))
            {
                return null;
            }

            var user = new User();
            var hashedPassword = hasher
                .HashPassword(user, req.Password);

            user.Username = req.Username;
            user.HashedPassword = hashedPassword;
            user.DisplayName = req.Username;
            user.Role = "User";
            user.CreatedAt = DateTime.UtcNow;

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return user;
        }

        public async Task<MyInfoDTO?> FetchUserInfoAsync(int userId)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null) return null;
            return new MyInfoDTO { Username = user.Username, DisplayName = user.DisplayName };
        }
    }
}

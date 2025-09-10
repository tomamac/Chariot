using Chariot.Models;
using Chariot.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Chariot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        //public static User user = new();

        [HttpPost("register")]
        public async Task<ActionResult> Register(UserAuthDTO req)
        {
            //var hashedPassword = new PasswordHasher<User>()
            //    .HashPassword(user, req.Password);

            //user.Username = req.Username;
            //user.PasswordHash = hashedPassword;

            var user = await authService.RegisterAsync(req);

            if (user is null)
            {
                return BadRequest("Username already exists");
            }

            return Ok(new { message = "Registration completed" });
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDTO>> Login(UserAuthDTO req)
        {
            //if (user.Username != req.Username)
            //{
            //    return BadRequest("Not found");
            //}
            //if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, req.Password)
            //    == PasswordVerificationResult.Failed)
            //{
            //    return BadRequest("Wrong credentials");
            //}

            //string token = CreateToken(user);

            var res = await authService.LoginAsync(req);

            if (res is null)
            {
                return BadRequest("Wrong credentials");
            }

            Response.Cookies.Append("access_token", res.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
            });

            Response.Cookies.Append("refresh_token", res.RefreshToken!, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
            });

            return Ok();
        }

        [HttpPost("guest-login")]
        public async Task<ActionResult<TokenResponseDTO>> GuestLogin(GuestLoginDTO req)
        {
            var res = await authService.LoginGuestAsync(req.Name);

            if (res is null)
            {
                return StatusCode(500, "Server error, please try again.");
            }

            Response.Cookies.Append("access_token", res.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
            });

            return Ok();
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if (!int.TryParse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
                return Unauthorized();

            if (!await authService.LogoutAsync(userId))
                return NotFound();

            Response.Cookies.Delete("access_token");

            return Ok(new { message = "Logged out successfully" });
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDTO>> RefreshToken()
        {
            if (!(Request.Cookies.TryGetValue("access_token", out var accessToken) &&
                Request.Cookies.TryGetValue("refresh_token", out var refreshToken)))
                return Unauthorized("Invalid token");

            var req = new RefreshTokenRequestDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
            var res = await authService.RefreshTokenAsync(req);
            if (res is null || res.AccessToken is null || res.RefreshToken is null) return Unauthorized("Invalid token");

            Response.Cookies.Append("access_token", res.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
            });

            Response.Cookies.Append("refresh_token", res.RefreshToken!, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
            });

            return Ok(res);
        }

        [HttpGet("me")]
        public async Task<ActionResult<MyInfoDTO>> GetMe()
        {
            if (!int.TryParse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
                return Unauthorized();

            var user = await authService.FetchUserInfoAsync(userId);

            if (user is null) return NotFound();

            return Ok(user);
        }

        [Authorize]
        [HttpGet]
        public IActionResult ProtectedRoute()
        {
            return Ok("Protected route accessed");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult AdminRoute()
        {
            return Ok("Admin route accessed");
        }
    }
}

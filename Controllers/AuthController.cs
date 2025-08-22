using Chariot.Entities;
using Chariot.Models;
using Chariot.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

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

            return Ok(new { message = "Registration completed"});
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

            return Ok(res);
        }

        [HttpPost("guest-login")]
        public async Task<ActionResult<TokenResponseDTO>> GuestLogin(GuestLoginDTO req)
        {
            var res = await authService.LoginGuestAsync(req.Name);

            if(res is null)
            {
                return StatusCode(500, "Server error, please try again.");
            }
            return Ok(res);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDTO>> RefreshToken(RefreshTokenRequestDTO req)
        {
            var res = await authService.RefreshTokenAsync(req);
            if (res is null || res.AccessToken is null || res.RefreshToken is null) return Unauthorized("Invalid refresh token");

            return Ok(res);
        }

        //"app.use("/api", protectedRoutes);
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

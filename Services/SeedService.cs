using Chariot.Data;
using Chariot.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Chariot.Services
{
    public class SeedService(ChariotDbContext context, IConfiguration configuration, IPasswordHasher<User> hasher) : ISeedService
    {
        public async Task<User?> AdminSeedAsync()
        {
            var admin = new User();
            admin.Username = configuration["ADMIN_USER"]!;
            admin.HashedPassword = hasher.HashPassword(admin, configuration["ADMIN_PASS"]!);
            admin.DisplayName = "Tomamac";
            admin.Role = "Admin";
            admin.CreatedAt = DateTime.UtcNow;

            if (await context.Users.AnyAsync(u => u.Username == admin.Username))
            {
                return null;
            }

            context.Users.Add(admin);
            await context.SaveChangesAsync();

            return admin;
        }
    }
}

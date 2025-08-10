using Chariot.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chariot.Data
{
    public class ChariotDbContext(DbContextOptions<ChariotDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Chatroom> Chatrooms { get; set; }
        public DbSet<ChatroomUser> ChatroomsUser { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChatroomUser>()
                .HasKey(t => new { t.UserId, t.ChatroomId });

            modelBuilder.Entity<ChatroomUser>()
                .HasOne(t => t.User)
                .WithMany(u => u.ChatroomUsers)
                .HasForeignKey(t => t.UserId);

            modelBuilder.Entity<ChatroomUser>()
                .HasOne(t => t.Chatroom)
                .WithMany(c => c.ChatroomUsers)
                .HasForeignKey(t => t.ChatroomId);
        }
    }
}

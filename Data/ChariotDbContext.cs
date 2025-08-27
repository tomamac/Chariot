using Chariot.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chariot.Data
{
    public class ChariotDbContext(DbContextOptions<ChariotDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Chatroom> Chatrooms { get; set; }
        public DbSet<ChatroomUser> ChatroomsUser { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //ChatroomUser Composite key
            modelBuilder.Entity<ChatroomUser>()
                .HasKey(t => new { t.UserId, t.ChatroomId });

            //ChatroomUser FK (UserId, ChatroomId) 1-TO-M
            modelBuilder.Entity<ChatroomUser>()
                .HasOne(t => t.User)
                .WithMany(u => u.ChatroomUsers)
                .HasForeignKey(t => t.UserId);

            modelBuilder.Entity<ChatroomUser>()
                .HasOne(t => t.Chatroom)
                .WithMany(c => c.ChatroomUsers)
                .HasForeignKey(t => t.ChatroomId);

            //Unique constraint
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Chatroom>()
                .HasIndex(c => c.Code)
                .IsUnique();

            //Messages FK (UserId, ChatroomId) 1-TO-M
            modelBuilder.Entity<Message>()
                .HasOne(t => t.User)
                .WithMany(u => u.Messages)
                .HasForeignKey(t => t.UserId);

            modelBuilder.Entity<Message>()
                .HasOne(t => t.Chatroom)
                .WithMany(c => c.Messages)
                .HasForeignKey(t => t.ChatroomId);
        }
    }
}

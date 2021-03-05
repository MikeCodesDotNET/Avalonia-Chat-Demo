using Bellow.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Bellow.Server
{
    public class ChatContext : DbContext
    {
        public ChatContext(DbContextOptions<ChatContext> options) : base(options)
        {
            Database.EnsureCreatedAsync();
        }

        public DbSet<User> Users { get; set; }

        public DbSet<MessagePayload> Messages { get; set; }

        public DbSet<ImageUrls> Images { get; set; }

        public DbSet<AccessToken> Tokens { get; set; }

        public DbSet<PassCode> PassCodes { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder options)
           => options.UseSqlite("Data Source=bellow.db");

    }
}

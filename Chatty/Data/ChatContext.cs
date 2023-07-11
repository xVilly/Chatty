using Microsoft.EntityFrameworkCore;
using Chatty.Models;

namespace Chatty.Data
{
    public class ChatContext : DbContext
    {
        public ChatContext(DbContextOptions<ChatContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
    }
}

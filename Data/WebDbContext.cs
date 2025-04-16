using Microsoft.EntityFrameworkCore;

namespace NotificationWebsite.Data
{
    public class WebDbContext : DbContext
    {
        public WebDbContext(DbContextOptions<WebDbContext> options) : base(options)
        {

        }

        public DbSet<User>? Users { get; set; }
    }
}
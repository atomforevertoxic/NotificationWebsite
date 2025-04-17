using Microsoft.EntityFrameworkCore;
using NotificationWebsite.Models;

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
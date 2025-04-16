using Microsoft.EntityFrameworkCore;

public class WebDbContext : DbContext
{
    public WebDbContext(DbContextOptions<WebDbContext> options) : base(options)
    {

    }

    public DbSet<User>? Users { get; set; }
}
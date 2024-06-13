using Microsoft.EntityFrameworkCore;

namespace WebThree.api;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    
    public DbSet<File> Files { get; set; }
}
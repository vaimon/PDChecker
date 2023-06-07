using Microsoft.EntityFrameworkCore;
using PDChecker.Models;

namespace PDChecker;

public class PdDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = default!;
    
    public DbSet<Project> Projects { get; set; } = default!;
    
    
    
    public PdDbContext (DbContextOptions<PdDbContext> options)
        : base(options)
    {
    }
}
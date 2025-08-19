using Microsoft.EntityFrameworkCore;
using Taskly.Api.Models;

namespace Taskly.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<User> Users => Set<User>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed one demo workspace
        modelBuilder.Entity<Workspace>().HasData(new Workspace
        {
            Id = 1,
            Name = "Demo Workspace"
        });
    }
}

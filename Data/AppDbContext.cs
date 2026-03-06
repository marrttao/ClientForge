namespace ClientForge.Data;
using Microsoft.EntityFrameworkCore;
using ClientForge.Features.User.Models;
using ClientForge.Features.Project.Models;


public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; private set; }
    public DbSet<Project> Projects { get; private set; }
    public DbSet<TaskModel> Tasks { get; private set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Связь: User (Client) -> Projects (один ко многим)
        modelBuilder.Entity<Project>()
            .HasOne(p => p.Client)
            .WithMany(u => u.Projects)
            .HasForeignKey(p => p.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Связь: Project -> Tasks (один ко многим)
        modelBuilder.Entity<TaskModel>()
            .HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Связь: User (Worker) -> Tasks (один ко многим)
        modelBuilder.Entity<TaskModel>()
            .HasOne(t => t.Worker)
            .WithMany(u => u.Tasks)
            .HasForeignKey(t => t.WorkerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
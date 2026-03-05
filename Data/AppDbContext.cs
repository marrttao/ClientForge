namespace ClientForge.Data;
using Microsoft.EntityFrameworkCore;
using ClientForge.Features.User.Models;


public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; private set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(builder =>
        {
            builder.ToTable("Users");
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Login)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Surname)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Role)
                .HasConversion<int>()
                .HasDefaultValue(Role.guest);

            builder.Property<string>("HashedPassword")
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(u => u.Login).IsUnique();
            builder.HasIndex(u => u.Email).IsUnique();
        });
    }
}
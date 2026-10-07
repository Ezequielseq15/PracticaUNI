using GraduacionUni.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GraduacionUni.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ProjectDocument> ProjectDocuments => Set<ProjectDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.Property(x => x.Email).HasMaxLength(150);
            e.Property(x => x.PasswordHash).HasMaxLength(500);
            e.Property(x => x.Role).HasMaxLength(30);
            e.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<Project>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(250);
            e.Property(x => x.Status).HasMaxLength(30);

            e.HasOne(x => x.Student)
                .WithMany(x => x.Projects)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Tutor)
                .WithMany(x => x.TutoredProjects)
                .HasForeignKey(x => x.TutorId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.StudentId);
            e.HasIndex(x => x.TutorId);
        });

        modelBuilder.Entity<Review>(e =>
        {
            e.Property(x => x.Comments).HasMaxLength(2000);

            e.HasOne(x => x.Project)
                .WithMany(x => x.Reviews)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Tutor)
                .WithMany()
                .HasForeignKey(x => x.TutorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProjectDocument>(e =>
        {
            e.Property(x => x.FileName).HasMaxLength(260);
            e.Property(x => x.StoredName).HasMaxLength(100);
            e.Property(x => x.ContentType).HasMaxLength(100);

            e.HasOne(x => x.Project)
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

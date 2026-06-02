using DevDocs.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace DevDocs.Infrastructure.Persistence;

public class DevDocsDbContext : DbContext
{
    public DevDocsDbContext(DbContextOptions<DevDocsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(builder =>
        {
            builder.ToTable("Projects");

            builder.HasKey(project => project.Id);

            builder.Property(project => project.Name)
                .IsRequired()
                .HasMaxLength(120);

            builder.Property(project => project.Owner)
                .IsRequired()
                .HasMaxLength(120);

            builder.Property(project => project.RepositoryName)
                .IsRequired()
                .HasMaxLength(120);

            builder.Property(project => project.GitHubUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(project => project.DefaultBranch)
                .IsRequired()
                .HasMaxLength(120);

            builder.Property(project => project.Description)
                .HasMaxLength(1000);

            builder.Property(project => project.CreatedAt)
                .IsRequired();

            builder.HasIndex(project => project.GitHubUrl)
                .IsUnique();
        });
    }
}
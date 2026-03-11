using Microsoft.EntityFrameworkCore;
using ExampleApi.Features.Articles.Shared.Models;

namespace ExampleApi.Infrastructure.Database;

/// <summary>
/// Application database context for managing articles.
/// </summary>
/// <param name="options">The database context options.</param>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets the articles database set.
    /// </summary>
    public DbSet<Article> Articles => Set<Article>();

    /// <summary>
    /// Configures the entity models and their relationships.
    /// </summary>
    /// <param name="modelBuilder">The model builder instance.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(article => article.ArticleId);

            entity.Property(article => article.ArticleId)
                .ValueGeneratedOnAdd();

            entity.Property(article => article.Name)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(article => article.Description)
                .IsRequired()
                .HasMaxLength(2048);

            entity.Property(article => article.Category)
                .HasMaxLength(64);

            entity.Property(article => article.Price)
                .HasColumnType("decimal(18,2)");

            entity.Property(article => article.Currency)
                .HasMaxLength(3);

            entity.Property(article => article.RowVersion)
                .IsRowVersion()
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken()
                .HasDefaultValueSql("randomblob(8)");
        });
    }
}
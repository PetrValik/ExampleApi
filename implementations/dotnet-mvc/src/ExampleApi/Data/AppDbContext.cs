using ExampleApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ExampleApi.Data;

/// <summary>
/// EF Core database context. Only the repository layer touches this type — controllers never do.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Article> Articles => Set<Article>();

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
                .HasColumnType("numeric(18,2)");

            entity.Property(article => article.Currency)
                .HasMaxLength(3);

            // Map RowVersion onto PostgreSQL's built-in xmin system column. Npgsql increments
            // xmin on every UPDATE, giving us a zero-storage optimistic-concurrency token.
            entity.Property(article => article.RowVersion)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsRowVersion()
                .IsConcurrencyToken();
        });
    }
}

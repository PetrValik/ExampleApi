using ExampleApi.Features.Articles.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace ExampleApi.Infrastructure.Database;

/// <summary>
/// The EF Core context. A single <c>Articles</c> table whose optimistic-concurrency
/// token is the PostgreSQL <c>xmin</c> system column (a zero-storage row version that
/// Postgres bumps on every UPDATE).
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>The articles table.</summary>
    public DbSet<Article> Articles => Set<Article>();

    /// <inheritdoc />
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

            // xmin: a PostgreSQL system column bumped on every row modification —
            // used as a free optimistic-concurrency token. Npgsql omits it from
            // CREATE TABLE (it is implicit) but reads it back after INSERT/UPDATE.
            entity.Property(article => article.RowVersion)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsRowVersion()
                .IsConcurrencyToken();
        });
    }
}

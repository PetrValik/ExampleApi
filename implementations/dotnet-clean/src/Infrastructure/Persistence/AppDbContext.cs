using ExampleApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExampleApi.Infrastructure.Persistence;

/// <summary>
/// EF Core context for the article store. Maps the encapsulated <see cref="Article"/>
/// aggregate (private setters, private constructor) to the <c>articles</c> table.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Article> Articles => Set<Article>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Article>(entity =>
        {
            entity.ToTable("articles");

            entity.HasKey(article => article.ArticleId);
            entity.Property(article => article.ArticleId).ValueGeneratedOnAdd();

            entity.Property(article => article.Name).IsRequired().HasMaxLength(64);
            entity.Property(article => article.Description).IsRequired().HasMaxLength(2048);
            entity.Property(article => article.Category).HasMaxLength(64);
            entity.Property(article => article.Price).HasColumnType("numeric(18,2)");
            entity.Property(article => article.Currency).HasMaxLength(3);

            // Portable optimistic-concurrency counter: a plain bigint the domain increments
            // on each update. The Application layer compares the caller-supplied value
            // against the current one to decide 200 vs 409.
            entity.Property(article => article.RowVersion).IsRequired();

            // Money is a projection over Price + Currency, not its own column.
            entity.Ignore(article => article.Money);
        });
    }
}

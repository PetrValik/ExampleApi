using ExampleApi.Domain.ValueObjects;

namespace ExampleApi.Domain.Entities;

/// <summary>
/// The Article aggregate root. State is fully encapsulated: callers mutate it only
/// through <see cref="Create"/> and <see cref="Update"/>, which keeps the optimistic
/// concurrency counter (<see cref="RowVersion"/>) honest.
/// </summary>
/// <remarks>
/// <para>
/// <b>Concurrency model.</b> <see cref="RowVersion"/> is a monotonically increasing
/// integer that starts at <c>1</c> on creation and is incremented by <see cref="Update"/>
/// on every successful update. This is the portable "version column" approach: a caller
/// echoes back the version it last read, and the Application layer rejects a write whose
/// version no longer matches the current one with a 409 Conflict.
/// </para>
/// </remarks>
public sealed class Article
{
    // Private, parameterless constructor for the EF Core materializer only.
    private Article()
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    /// <summary>The database-generated identifier.</summary>
    public int ArticleId { get; private set; }

    /// <summary>The article name (1–64 characters, enforced by the Application validators).</summary>
    public string Name { get; private set; }

    /// <summary>The article description (1–2048 characters).</summary>
    public string Description { get; private set; }

    /// <summary>The optional category (≤64 characters).</summary>
    public string? Category { get; private set; }

    /// <summary>The price amount.</summary>
    public decimal Price { get; private set; }

    /// <summary>The ISO 4217 currency code, or <c>null</c> for a free article.</summary>
    public string? Currency { get; private set; }

    /// <summary>
    /// The optimistic-concurrency token. Starts at 1 and increments on every update.
    /// </summary>
    public long RowVersion { get; private set; }

    /// <summary>
    /// The price expressed as a <see cref="Money"/> value object. Not persisted directly;
    /// it is projected from <see cref="Price"/> and <see cref="Currency"/>.
    /// </summary>
    public Money Money => new(Price, Currency);

    /// <summary>
    /// Factory for a brand-new article. The row version starts at 1.
    /// </summary>
    public static Article Create(string name, string description, string? category, Money price)
    {
        return new Article
        {
            Name = name,
            Description = description,
            Category = NormaliseCategory(category),
            Price = price.Amount,
            Currency = price.Currency,
            RowVersion = 1
        };
    }

    /// <summary>
    /// Replaces the mutable fields and advances the concurrency token.
    /// </summary>
    public void Update(string name, string description, string? category, Money price)
    {
        Name = name;
        Description = description;
        Category = NormaliseCategory(category);
        Price = price.Amount;
        Currency = price.Currency;
        RowVersion += 1;
    }

    private static string? NormaliseCategory(string? category) =>
        string.IsNullOrWhiteSpace(category) ? null : category;
}

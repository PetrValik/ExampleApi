namespace ExampleApi.Domain.ValueObjects;

/// <summary>
/// A value object pairing a monetary <see cref="Amount"/> with its optional
/// <see cref="Currency"/>. A free article is modelled as amount <c>0</c> with a
/// <c>null</c> currency. Structural equality is provided by the record.
/// </summary>
/// <param name="Amount">The price amount. Never negative for a valid article.</param>
/// <param name="Currency">The ISO 4217 currency code, or <c>null</c> for a free article.</param>
public readonly record struct Money(decimal Amount, string? Currency)
{
    /// <summary>
    /// Creates a free (zero-cost, currency-less) money value.
    /// </summary>
    public static Money Free { get; } = new(0m, null);

    /// <summary>
    /// Indicates whether this money represents a free article.
    /// </summary>
    public bool IsFree => Amount == 0m;
}

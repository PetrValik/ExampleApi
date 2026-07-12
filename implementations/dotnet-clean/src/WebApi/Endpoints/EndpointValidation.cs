using FluentValidation;

namespace ExampleApi.WebApi.Endpoints;

/// <summary>
/// Runs a FluentValidation validator and, on failure, produces a 400
/// <c>application/problem+json</c> response whose <c>errors</c> map is keyed by property
/// name — the contract's required validation shape.
/// </summary>
internal static class EndpointValidation
{
    /// <summary>
    /// Returns <c>null</c> when valid, or a 400 ValidationProblem result when not.
    /// </summary>
    public static async Task<IResult?> ValidateAsync<T>(
        IValidator<T> validator,
        T request,
        CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(request, cancellationToken);
        if (result.IsValid)
        {
            return null;
        }

        var errors = result.Errors
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).ToArray());

        return Results.ValidationProblem(errors);
    }
}

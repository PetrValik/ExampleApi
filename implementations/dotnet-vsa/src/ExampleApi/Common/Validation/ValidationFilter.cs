using FluentValidation;

namespace ExampleApi.Common.Validation;

/// <summary>
/// Provides validation filtering functionality for API requests.
/// </summary>
public static class ValidationFilter
{
    /// <summary>
    /// Validates a request using FluentValidation and returns validation errors if any.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request to validate.</typeparam>
    /// <param name="validator">The FluentValidation validator instance.</param>
    /// <param name="request">The request instance to validate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// Null if validation succeeds, otherwise a validation problem result with grouped error messages.
    /// </returns>
    public static async Task<IResult?> ValidateAsync<TRequest>(
        IValidator<TRequest> validator,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (validationResult.IsValid)
        {
            return null;
        }

        var errors = validationResult.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());

        return Results.ValidationProblem(errors);
    }
}
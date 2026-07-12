using System.Reflection;
using ExampleApi.Common.Results;
using FluentValidation;
using MediatR;

namespace ExampleApi.Common.Behaviors;

/// <summary>
/// A MediatR pipeline behaviour that runs every registered FluentValidation validator
/// for the incoming request and, on failure, short-circuits the pipeline by returning a
/// <see cref="Result"/> failure carrying a <see cref="ValidationError"/> — the handler is
/// never invoked. The endpoint layer turns that failure into a 400 problem+json response.
/// </summary>
/// <typeparam name="TRequest">The MediatR request type.</typeparam>
/// <typeparam name="TResponse">The response type, constrained to <see cref="Result"/>.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(validationResult => validationResult.Errors)
            .Where(failure => failure is not null)
            .ToArray();

        if (failures.Length == 0)
        {
            return await next();
        }

        var errors = failures
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).ToArray());

        return CreateValidationFailure(new ValidationError(errors));
    }

    /// <summary>
    /// Builds a failed <typeparamref name="TResponse"/> — either a non-generic
    /// <see cref="Result"/> or a <see cref="Result{TValue}"/> — from a validation error.
    /// </summary>
    private static TResponse CreateValidationFailure(ValidationError error)
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        var valueType = typeof(TResponse).GetGenericArguments()[0];

        var failureMethod = typeof(Result)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(method => method is { Name: nameof(Result.Failure), IsGenericMethod: true })
            .MakeGenericMethod(valueType);

        return (TResponse)failureMethod.Invoke(null, [error])!;
    }
}

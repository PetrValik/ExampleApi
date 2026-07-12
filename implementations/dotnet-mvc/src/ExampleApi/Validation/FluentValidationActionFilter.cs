using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ExampleApi.Validation;

/// <summary>
/// Global MVC action filter that runs any registered FluentValidation validator against each bound
/// action argument. On failure it short-circuits with a normalised
/// <c>400 application/problem+json</c> body carrying an <c>errors</c> map — never the framework's
/// default validation envelope.
/// </summary>
public sealed class FluentValidationActionFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (serviceProvider.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);
            if (result.IsValid)
            {
                continue;
            }

            var errors = result.Errors
                .GroupBy(failure => failure.PropertyName)
                .ToDictionary(
                    group => NormaliseKey(group.Key),
                    group => group.Select(failure => failure.ErrorMessage).ToArray());

            var problemDetails = new ValidationProblemDetails(errors)
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
            };

            context.Result = new ObjectResult(problemDetails)
            {
                StatusCode = StatusCodes.Status400BadRequest,
                ContentTypes = { "application/problem+json" },
            };

            return;
        }

        await next();
    }

    /// <summary>
    /// Collection-level rules (e.g. on the batch list) report an empty property name; give them a
    /// stable key so the errors map is never keyed on "".
    /// </summary>
    private static string NormaliseKey(string propertyName) =>
        string.IsNullOrEmpty(propertyName) ? "request" : propertyName;
}

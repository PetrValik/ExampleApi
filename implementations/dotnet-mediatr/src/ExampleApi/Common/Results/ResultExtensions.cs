using Microsoft.AspNetCore.Http;

// The current namespace segment "Results" shadows Microsoft.AspNetCore.Http.Results,
// so alias the Minimal-API results factory to call it unambiguously here.
using Http = Microsoft.AspNetCore.Http.Results;

namespace ExampleApi.Common.Results;

/// <summary>
/// Maps a failed <see cref="Result"/> onto an RFC 7807 <c>application/problem+json</c>
/// HTTP response. Validation failures become a 400 with an <c>errors</c> map; other
/// failures become a plain problem document with the appropriate status.
/// </summary>
public static class ResultExtensions
{
    /// <summary>Converts a failed result into a problem+json <see cref="IResult"/>.</summary>
    public static IResult ToProblem(this Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("A successful result cannot be converted to a problem response.");
        }

        // Validation failures carry a per-field error map → 400 with an "errors" object.
        if (result.Error is ValidationError validationError)
        {
            return Http.ValidationProblem(validationError.Errors);
        }

        return result.Error.Type switch
        {
            ErrorType.NotFound => Problem(StatusCodes.Status404NotFound, "Not Found", result.Error.Description),
            ErrorType.Conflict => Problem(StatusCodes.Status409Conflict, "Conflict", result.Error.Description),
            ErrorType.Validation => Http.ValidationProblem(new Dictionary<string, string[]>()),
            _ => Problem(StatusCodes.Status400BadRequest, "Bad Request", result.Error.Description)
        };
    }

    private static IResult Problem(int statusCode, string title, string? detail) =>
        Http.Problem(
            statusCode: statusCode,
            title: title,
            detail: detail,
            type: $"https://httpstatuses.io/{statusCode}");
}

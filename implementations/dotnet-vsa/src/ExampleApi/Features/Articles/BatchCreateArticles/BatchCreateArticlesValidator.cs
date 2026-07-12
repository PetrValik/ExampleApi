using FluentValidation;
using ExampleApi.Features.Articles.Shared.DTOs;
using ExampleApi.Features.Articles.Shared.Validators;

namespace ExampleApi.Features.Articles.BatchCreateArticles;

/// <summary>
/// Validator for concurrent article creation requests.
/// </summary>
public sealed class BatchCreateArticlesValidator : AbstractValidator<List<ArticleRequest>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BatchCreateArticlesValidator"/> class.
    /// </summary>
    public BatchCreateArticlesValidator()
    {
        RuleFor(request => request)
            .NotEmpty()
            .WithMessage("At least one article is required.")
            .Must(list => list.Count <= 100)
            .WithMessage("Cannot create more than 100 articles at once.");

        RuleForEach(request => request)
            .SetValidator(new ArticleRequestValidator());
    }
}

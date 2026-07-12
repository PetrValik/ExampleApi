using ExampleApi.Dtos;
using FluentValidation;

namespace ExampleApi.Validation;

/// <summary>
/// Validates the batch-create payload: 1–100 items, each a valid <see cref="ArticleRequest"/>.
/// An empty array or any invalid item yields a single 400.
/// </summary>
public sealed class BatchCreateArticlesValidator : AbstractValidator<List<ArticleRequest>>
{
    public BatchCreateArticlesValidator()
    {
        RuleFor(list => list)
            .NotEmpty().WithMessage("At least one article is required.")
            .Must(list => list.Count <= 100).WithMessage("Cannot create more than 100 articles at once.");

        RuleForEach(list => list).SetValidator(new ArticleRequestValidator());
    }
}

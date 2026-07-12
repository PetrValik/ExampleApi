using ExampleApi.Application.Articles.Dtos;
using FluentValidation;

namespace ExampleApi.Application.Articles.Validation;

/// <summary>
/// Validates a batch-create payload: the array must be non-empty, hold at most 100
/// items, and every item must itself be valid.
/// </summary>
public sealed class BatchCreateArticlesValidator : AbstractValidator<List<CreateArticleRequest>>
{
    public BatchCreateArticlesValidator()
    {
        RuleFor(list => list)
            .NotEmpty().WithMessage("At least one article is required.")
            .Must(list => list.Count <= 100).WithMessage("Cannot create more than 100 articles at once.");

        RuleForEach(list => list).SetValidator(new CreateArticleRequestValidator());
    }
}

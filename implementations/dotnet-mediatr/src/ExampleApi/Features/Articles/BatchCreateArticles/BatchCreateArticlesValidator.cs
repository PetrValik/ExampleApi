using ExampleApi.Features.Articles.Shared.Validators;
using FluentValidation;

namespace ExampleApi.Features.Articles.BatchCreateArticles;

/// <summary>
/// Validates a batch: non-empty, at most 100 items, and every item valid per the shared rules.
/// </summary>
public sealed class BatchCreateArticlesValidator : AbstractValidator<BatchCreateArticlesCommand>
{
    private const int MaxBatchSize = 100;

    /// <summary>Initialises the validator.</summary>
    public BatchCreateArticlesValidator()
    {
        RuleFor(command => command.Items)
            .NotEmpty().WithMessage("At least one article is required.");

        RuleFor(command => command.Items)
            .Must(items => items.Count <= MaxBatchSize)
            .WithMessage($"A maximum of {MaxBatchSize} articles can be created at once.")
            .When(command => command.Items is not null);

        RuleForEach(command => command.Items)
            .SetValidator(new ArticleRequestValidator());
    }
}

using ExampleApi.Features.Articles.Shared.Validators;
using FluentValidation;

namespace ExampleApi.Features.Articles.CreateArticle;

/// <summary>
/// Validates a <see cref="CreateArticleCommand"/> by delegating to the shared field rules.
/// </summary>
public sealed class CreateArticleValidator : AbstractValidator<CreateArticleCommand>
{
    /// <summary>Initialises the validator.</summary>
    public CreateArticleValidator()
    {
        RuleFor(command => command.Article)
            .NotNull().WithMessage("A request body is required.")
            .SetValidator(new ArticleRequestValidator());
    }
}

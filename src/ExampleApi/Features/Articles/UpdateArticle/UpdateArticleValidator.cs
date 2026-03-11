using FluentValidation;
using ExampleApi.Common.Currency;
using ExampleApi.Features.Articles.Shared.DTOs;

namespace ExampleApi.Features.Articles.UpdateArticle;

/// <summary>
/// Validator for update article requests.
/// </summary>
public sealed class UpdateArticleValidator : AbstractValidator<UpdateArticleRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateArticleValidator"/> class.
    /// </summary>
    public UpdateArticleValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(64)
            .WithMessage("Name must not exceed 64 characters.");

        RuleFor(request => request.Description)
            .NotEmpty()
            .WithMessage("Description is required.")
            .MaximumLength(2048)
            .WithMessage("Description must not exceed 2048 characters.");

        RuleFor(request => request.Category)
            .MaximumLength(64)
            .WithMessage("Category must not exceed 64 characters.")
            .When(request => !string.IsNullOrEmpty(request.Category));

        RuleFor(request => request.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price must be greater than or equal to 0.");

        RuleFor(request => request.Currency)
            .NotEmpty()
            .WithMessage("Currency is required when price is greater than 0.")
            .Length(3)
            .WithMessage("Currency must be a valid ISO 4217 code (3 characters).")
            .Must(CurrencyCodes.IsSupported)
            .WithMessage("Currency must be a supported currency code.")
            .When(request => request.Price > 0);

        RuleFor(request => request.RowVersion)
            .NotEmpty()
            .WithMessage("Row version is required for concurrency control.");
    }
}

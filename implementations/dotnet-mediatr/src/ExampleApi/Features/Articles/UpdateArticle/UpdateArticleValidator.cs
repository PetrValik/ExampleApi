using ExampleApi.Common.Currency;
using FluentValidation;

namespace ExampleApi.Features.Articles.UpdateArticle;

/// <summary>
/// Validates an <see cref="UpdateArticleCommand"/>: the same field rules as create, plus a
/// required non-zero <c>row_version</c>.
/// </summary>
public sealed class UpdateArticleValidator : AbstractValidator<UpdateArticleCommand>
{
    /// <summary>Initialises the validator.</summary>
    public UpdateArticleValidator()
    {
        RuleFor(command => command.Request.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(64).WithMessage("Name must not exceed 64 characters.");

        RuleFor(command => command.Request.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(2048).WithMessage("Description must not exceed 2048 characters.");

        RuleFor(command => command.Request.Category)
            .MaximumLength(64).WithMessage("Category must not exceed 64 characters.")
            .When(command => !string.IsNullOrEmpty(command.Request.Category));

        RuleFor(command => command.Request.Price)
            .GreaterThanOrEqualTo(0m).WithMessage("Price must be greater than or equal to 0.")
            .LessThanOrEqualTo(9_999_999_999_999_999.99m)
            .WithMessage("Price must not exceed 9,999,999,999,999,999.99.");

        RuleFor(command => command.Request.Currency)
            .NotEmpty().WithMessage("Currency is required when price is greater than 0.")
            .Length(3).WithMessage("Currency must be a valid ISO 4217 code (3 characters).")
            .Must(CurrencyCodes.IsSupported).WithMessage("Currency must be a supported currency code.")
            .When(command => command.Request.Price > 0m);

        RuleFor(command => command.Request.RowVersion)
            .Must(rowVersion => rowVersion is > 0u)
            .WithMessage("Row version is required and must be a non-zero value.");
    }
}

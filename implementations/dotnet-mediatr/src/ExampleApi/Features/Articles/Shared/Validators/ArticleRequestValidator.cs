using ExampleApi.Common.Currency;
using ExampleApi.Features.Articles.Shared.Dtos;
using FluentValidation;

namespace ExampleApi.Features.Articles.Shared.Validators;

/// <summary>
/// Field-level rules shared by create and batch-create (via composition with the
/// command validators). Currency is required and checked against the supported set
/// only when the price is greater than zero.
/// </summary>
public sealed class ArticleRequestValidator : AbstractValidator<ArticleRequest>
{
    /// <summary>Initialises the validator.</summary>
    public ArticleRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(64).WithMessage("Name must not exceed 64 characters.");

        RuleFor(request => request.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(2048).WithMessage("Description must not exceed 2048 characters.");

        RuleFor(request => request.Category)
            .MaximumLength(64).WithMessage("Category must not exceed 64 characters.")
            .When(request => !string.IsNullOrEmpty(request.Category));

        RuleFor(request => request.Price)
            .GreaterThanOrEqualTo(0m).WithMessage("Price must be greater than or equal to 0.")
            .LessThanOrEqualTo(9_999_999_999_999_999.99m)
            .WithMessage("Price must not exceed 9,999,999,999,999,999.99.");

        RuleFor(request => request.Currency)
            .NotEmpty().WithMessage("Currency is required when price is greater than 0.")
            .Length(3).WithMessage("Currency must be a valid ISO 4217 code (3 characters).")
            .Must(CurrencyCodes.IsSupported).WithMessage("Currency must be a supported currency code.")
            .When(request => request.Price > 0m);
    }
}

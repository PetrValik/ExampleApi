using ExampleApi.Application.Articles.Dtos;
using ExampleApi.Domain.ValueObjects;
using FluentValidation;

namespace ExampleApi.Application.Articles.Validation;

/// <summary>
/// Validates the create (and per-item batch) request. Every failure surfaces as an
/// entry in the 400 problem+json <c>errors</c> map, keyed by property name.
/// </summary>
public sealed class CreateArticleRequestValidator : AbstractValidator<CreateArticleRequest>
{
    public CreateArticleRequestValidator()
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
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0.")
            .LessThanOrEqualTo(9_999_999_999_999_999.99m).WithMessage("Price must not exceed 9,999,999,999,999,999.99.");

        // Currency is required and validated against the supported set ONLY when price > 0.
        RuleFor(request => request.Currency)
            .NotEmpty().WithMessage("Currency is required when price is greater than 0.")
            .Length(3).WithMessage("Currency must be a valid ISO 4217 code (3 characters).")
            .Must(CurrencyCode.IsSupported).WithMessage("Currency must be a supported currency code.")
            .When(request => request.Price > 0);
    }
}

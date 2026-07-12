using ExampleApi.Application.Articles.Dtos;
using ExampleApi.Domain.ValueObjects;
using FluentValidation;

namespace ExampleApi.Application.Articles.Validation;

/// <summary>
/// Validates the update request: the same field rules as create, plus a required
/// <c>row_version</c> that must be present and ≥ 1.
/// </summary>
public sealed class UpdateArticleRequestValidator : AbstractValidator<UpdateArticleRequest>
{
    public UpdateArticleRequestValidator()
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

        RuleFor(request => request.Currency)
            .NotEmpty().WithMessage("Currency is required when price is greater than 0.")
            .Length(3).WithMessage("Currency must be a valid ISO 4217 code (3 characters).")
            .Must(CurrencyCode.IsSupported).WithMessage("Currency must be a supported currency code.")
            .When(request => request.Price > 0);

        RuleFor(request => request.RowVersion)
            .Must(version => version.HasValue && version.Value >= 1)
            .WithMessage("row_version is required and must be a non-zero value.");
    }
}

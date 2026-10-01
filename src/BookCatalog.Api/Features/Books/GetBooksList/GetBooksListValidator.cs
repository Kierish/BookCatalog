using FluentValidation;

namespace BookCatalog.Api.Features.Books.GetBooksList
{
    public sealed class GetBooksListValidator : AbstractValidator<GetBooksListRequest>
    {
        public GetBooksListValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page must be at least 1.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 50)
                .WithMessage("PageSize must be between 1 and 50.");

            RuleFor(x => x.Title)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.Title))
                .WithMessage("Title must not exceed 200 characters.");

            RuleFor(x => x.Author)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.Author))
                .WithMessage("Author must not exceed 200 characters.");

            RuleFor(x => x.PublicationYear)
                .InclusiveBetween(1, DateTime.UtcNow.Year)
                .When(x => x.PublicationYear.HasValue)
                .WithMessage($"Publication year must be between 1 and {DateTime.UtcNow.Year}.");
        }
    }
}

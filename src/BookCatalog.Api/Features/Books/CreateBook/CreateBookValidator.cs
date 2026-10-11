using FluentValidation;
using BookCatalog.Api.Features.Books.Shared;

namespace BookCatalog.Api.Features.Books.CreateBook
{
    public sealed class CreateBookValidator : AbstractValidator<CreateBookRequest>
    {
        public CreateBookValidator()
        {
            RuleFor(x => x.Title)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Title is required.")
                .Length(1, 200)
                .WithMessage("Title must be between 1 and 200 characters.");

            RuleFor(x => x.AuthorId)
                .NotEmpty()
                .WithMessage("AuthorId is required.");

            RuleFor(x => x.Isbn)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(17)
                .WithMessage("ISBN must not exceed 17 characters.")
                .Must(IsbnValidator.IsValid)
                .WithMessage("ISBN must be a valid ISBN-10 or ISBN-13.");

            RuleFor(x => x.PublicationYear)
                .InclusiveBetween(1, DateTime.UtcNow.Year)
                .WithMessage($"Publication year must be between 1 and {DateTime.UtcNow.Year}.");
        }
    }
}

using BookCatalog.Api.Features.Books.GetBooksList;
using FluentValidation.TestHelper;

namespace BookCatalog.UnitTests.Features.Books.GetBooksList
{
    public sealed class GetBooksListValidatorTests
    {
        private readonly GetBooksListValidator _validator = new();

        #region Happy Path

        [Fact]
        public void Validate_WhenDefaultRequest_ShouldNotHaveAnyValidationErrors()
        {
            var request = new GetBooksListRequest();

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WhenAllFiltersArePopulatedAndValid_ShouldNotHaveAnyValidationErrors()
        {
            var request = new GetBooksListRequest(
                Title: "Clean Code",
                Author: "Robert C. Martin",
                PublicationYear: 2008,
                PageNumber: 2,
                PageSize: 25
            );

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        #endregion

        #region PageNumber

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WhenPageNumberIsLessThanOne_ShouldHaveValidationError(int invalidPageNumber)
        {
            var request = new GetBooksListRequest(PageNumber: invalidPageNumber);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.PageNumber)
                  .WithErrorMessage("Page must be at least 1.");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public void Validate_WhenPageNumberIsAtLeastOne_ShouldNotHaveValidationError(int validPageNumber)
        {
            var request = new GetBooksListRequest(PageNumber: validPageNumber);

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.PageNumber);
        }

        #endregion

        #region PageSize

        [Theory]
        [InlineData(0)]  
        [InlineData(51)] 
        public void Validate_WhenPageSizeIsOutOfRange_ShouldHaveValidationError(int invalidPageSize)
        {
            var request = new GetBooksListRequest(PageSize: invalidPageSize);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                  .WithErrorMessage("PageSize must be between 1 and 50.");
        }

        [Theory]
        [InlineData(1)]  
        [InlineData(25)] 
        [InlineData(50)] 
        public void Validate_WhenPageSizeIsWithinBoundaries_ShouldNotHaveValidationError(int validPageSize)
        {
            var request = new GetBooksListRequest(PageSize: validPageSize);

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
        }

        #endregion

        #region Title Filter

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Domain-Driven Design")]
        public void Validate_WhenTitleIsNullOrWhiteSpaceOrValidLength_ShouldNotHaveValidationError(string? title)
        {
            var request = new GetBooksListRequest(Title: title);

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Validate_WhenTitleExceeds200Characters_ShouldHaveValidationError()
        {
            var request = new GetBooksListRequest(Title: new string('a', 201));

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage("Title must not exceed 200 characters.");
        }

        #endregion

        #region Author Filter

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Martin Fowler")]
        public void Validate_WhenAuthorIsNullOrWhiteSpaceOrValidLength_ShouldNotHaveValidationError(string? author)
        {
            var request = new GetBooksListRequest(Author: author);

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.Author);
        }

        [Fact]
        public void Validate_WhenAuthorExceeds200Characters_ShouldHaveValidationError()
        {
            var request = new GetBooksListRequest(Author: new string('a', 201));

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Author)
                  .WithErrorMessage("Author must not exceed 200 characters.");
        }

        #endregion

        #region PublicationYear Filter

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WhenPublicationYearIsLessThanOne_ShouldHaveValidationError(int year)
        {
            var request = new GetBooksListRequest(PublicationYear: year);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.PublicationYear);
        }

        [Fact]
        public void Validate_WhenPublicationYearIsInFuture_ShouldHaveValidationError()
        {
            var futureYear = DateTime.UtcNow.Year + 1;
            var request = new GetBooksListRequest(PublicationYear: futureYear);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.PublicationYear);
        }

        [Theory]
        [MemberData(nameof(GetValidPublicationYears))]
        public void Validate_WhenPublicationYearIsWithinAllowedRangeOrNull_ShouldNotHaveValidationError(int? year)
        {
            var request = new GetBooksListRequest(PublicationYear: year);

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.PublicationYear);
        }

        public static TheoryData<int?> GetValidPublicationYears() => new()
        {
            null,                
            1,                   
            DateTime.UtcNow.Year  
        };

        #endregion
    }
}

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
                Search: "Clean Code",
                AuthorId: Guid.CreateVersion7(),
                PublicationYear: 2008,
                PageNumber: 2,
                PageSize: 25);

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
                  .WithErrorMessage("PageNumber must be at least 1.");
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

        #region Search

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Domain-Driven Design")]
        public void Validate_WhenSearchIsNullOrWhiteSpaceOrValidLength_ShouldNotHaveValidationError(string? search)
        {
            var request = new GetBooksListRequest(Search: search);

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.Search);
        }

        [Fact]
        public void Validate_WhenSearchExceeds200Characters_ShouldHaveValidationError()
        {
            var request = new GetBooksListRequest(Search: new string('a', 201));

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Search)
                  .WithErrorMessage("Search must not exceed 200 characters.");
        }

        #endregion

        #region AuthorId Filter

        [Fact]
        public void Validate_WhenAuthorIdIsNull_ShouldNotHaveValidationError()
        {
            var request = new GetBooksListRequest(AuthorId: null);

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.AuthorId);
        }

        [Fact]
        public void Validate_WhenAuthorIdIsValid_ShouldNotHaveValidationError()
        {
            var request = new GetBooksListRequest(AuthorId: Guid.CreateVersion7());

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.AuthorId);
        }

        [Fact]
        public void Validate_WhenAuthorIdIsEmpty_ShouldHaveValidationError()
        {
            var request = new GetBooksListRequest(AuthorId: Guid.Empty);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.AuthorId)
                  .WithErrorMessage("AuthorId must not be empty.");
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

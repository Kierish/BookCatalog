using BookCatalog.Api.Features.Books.CreateBook;
using BookCatalog.Api.Features.Books.UpdateBook;
using BookCatalog.Domain.Entities;
using BookCatalog.Api.Features.Books; 
using Shouldly;

namespace BookCatalog.UnitTests.Features.Books.Shared
{
    public sealed class BookMappingsTests
    {   
        #region ToResponse

        [Theory]
        [InlineData("978-0132350884")]
        [InlineData(null)]
        public void ToResponse_FromBook_ShouldMapAllFieldsToResponse(string? isbn)
        {
            var id = Guid.CreateVersion7();
            var book = new Book
            {
                Id = id,
                Title = "Clean Code",
                Author = "Robert C. Martin",
                Isbn = isbn,
                PublicationYear = 2008
            };

            var response = book.ToResponse();

            response.ShouldSatisfyAllConditions(
                () => response.Id.ShouldBe(book.Id),
                () => response.Title.ShouldBe(book.Title),
                () => response.Author.ShouldBe(book.Author),
                () => response.Isbn.ShouldBe(book.Isbn),
                () => response.PublicationYear.ShouldBe(book.PublicationYear));
        }

        #endregion

        #region ToEntity (CreateBookRequest)

        [Theory]
        [InlineData("978-0134494166")]
        [InlineData(null)]
        public void ToEntity_FromCreateRequest_ShouldMapAllFieldsAndGenerateId(string? isbn)
        {
            var request = new CreateBookRequest("Clean Architecture", "Robert C. Martin", isbn, 2017);

            var book = request.ToEntity();

            book.ShouldSatisfyAllConditions(
                () => book.Id.ShouldNotBe(Guid.Empty), 
                () => book.Title.ShouldBe(request.Title),
                () => book.Author.ShouldBe(request.Author),
                () => book.Isbn.ShouldBe(request.Isbn),
                () => book.PublicationYear.ShouldBe(request.PublicationYear));
        }

        #endregion

        #region ToEntity (UpdateBookRequest)

        [Theory]
        [InlineData("978-0134757599")]
        [InlineData(null)]
        public void ToEntity_FromUpdateRequest_ShouldMapAllFields(string? isbn)
        {
            var id = Guid.CreateVersion7();
            var request = new UpdateBookRequest("Refactoring", "Martin Fowler", isbn, 2018);

            var book = request.ToEntity(id);

            book.ShouldSatisfyAllConditions(
                () => book.Id.ShouldBe(id),
                () => book.Title.ShouldBe("Refactoring"),
                () => book.Author.ShouldBe("Martin Fowler"),
                () => book.Isbn.ShouldBe(isbn),
                () => book.PublicationYear.ShouldBe(2018));
        }

        #endregion
    }
}

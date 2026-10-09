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
            var author = new Author { Id = Guid.CreateVersion7(), Name = "Robert C. Martin" };

            var book = new Book
            {
                Id = Guid.CreateVersion7(),
                Title = "Clean Code",
                AuthorId = author.Id,
                Author = author,
                Isbn = isbn,
                PublicationYear = 2008
            };

            var response = book.ToResponse();

            response.ShouldSatisfyAllConditions(
                () => response.Id.ShouldBe(book.Id),
                () => response.Title.ShouldBe(book.Title),
                () => response.AuthorId.ShouldBe(author.Id),
                () => response.AuthorName.ShouldBe(author.Name),
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
            var authorId = Guid.CreateVersion7();
            var request = new CreateBookRequest("Clean Architecture", authorId, isbn, 2017);

            var book = request.ToEntity();

            book.ShouldSatisfyAllConditions(
                () => book.Id.ShouldNotBe(Guid.Empty), 
                () => book.Title.ShouldBe(request.Title),
                () => book.AuthorId.ShouldBe(request.AuthorId),
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
            var authorId = Guid.CreateVersion7();
            var request = new UpdateBookRequest("Refactoring", authorId, isbn, 2018);

            var book = request.ToEntity(id);

            book.ShouldSatisfyAllConditions(
                () => book.Id.ShouldBe(id),
                () => book.Title.ShouldBe("Refactoring"),
                () => book.AuthorId.ShouldBe(authorId),
                () => book.Isbn.ShouldBe(isbn),
                () => book.PublicationYear.ShouldBe(2018));
        }

        #endregion
    }
}

using System.Net;
using System.Net.Http.Json;
using BookCatalog.Api.Features.Books;
using BookCatalog.Api.Features.Books.CreateBook;
using BookCatalog.Domain.Entities;
using BookCatalog.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace BookCatalog.IntegrationTests.Features.Books
{
    [Collection(IntegrationTestCollection.Name)]
    public sealed class CreateBookTests : IntegrationTestBase
    {
        public CreateBookTests(IntegrationTestFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task CreateBook_WhenRequestIsValid_ShouldCreateAndReturnBook()
        {
            var author = new Author
            {
                Name = "Robert C. Martin"
            };

            await ExecuteDbContextAsync(async dbContext =>
            {
                dbContext.Authors.Add(author);
                await dbContext.SaveChangesAsync();
            });

            var request = new CreateBookRequest(
                Title: "Clean Architecture",
                AuthorId: author.Id,
                Isbn: "978-0134494166",
                PublicationYear: 2017);

            var response = await Client.PostAsJsonAsync("/api/books", request);

            response.StatusCode.ShouldBe(HttpStatusCode.Created);

            var createdBook = await response.Content
                .ReadFromJsonAsync<BookResponse>();

            createdBook.ShouldNotBeNull();
            createdBook.Id.ShouldNotBe(Guid.Empty);
            createdBook.Title.ShouldBe(request.Title);
            createdBook.AuthorId.ShouldBe(author.Id);
            createdBook.AuthorName.ShouldBe(author.Name);
            createdBook.Isbn.ShouldBe(request.Isbn);
            createdBook.PublicationYear.ShouldBe(request.PublicationYear);
            createdBook.IsBorrowed.ShouldBeFalse();

            response.Headers.Location.ShouldNotBeNull();
            response.Headers.Location.AbsolutePath
                .ShouldBe($"/api/books/{createdBook.Id}");

            await ExecuteDbContextAsync(async dbContext =>
            {
                var persistedBook = await dbContext.Books
                    .SingleAsync(book => book.Id == createdBook.Id);

                persistedBook.Title.ShouldBe(request.Title);
                persistedBook.AuthorId.ShouldBe(request.AuthorId);
                persistedBook.Isbn.ShouldBe(request.Isbn);
                persistedBook.PublicationYear.ShouldBe(request.PublicationYear);
            });
        }

        [Fact]
        public async Task CreateBook_WhenIsbnIsInvalid_ShouldReturnBadRequest()
        {
            var request = new CreateBookRequest(
                Title: "Clean Architecture",
                AuthorId: Guid.CreateVersion7(),
                Isbn: "978-0134494165",
                PublicationYear: 2017);

            var response = await Client.PostAsJsonAsync("/api/books", request);

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

            var problemDetails = await response.Content
                .ReadFromJsonAsync<HttpValidationProblemDetails>();

            problemDetails.ShouldNotBeNull();
            problemDetails.Title.ShouldBe("Validation Failed");
            problemDetails.Errors["Isbn"]
                .ShouldContain("ISBN must be a valid ISBN-10 or ISBN-13.");
        }

        [Fact]
        public async Task CreateBook_WhenAuthorDoesNotExist_ShouldReturnNotFound()
        {
            var authorId = Guid.CreateVersion7();
            var request = new CreateBookRequest(
                Title: "Clean Architecture",
                AuthorId: authorId,
                Isbn: "978-0134494166",
                PublicationYear: 2017);

            var response = await Client.PostAsJsonAsync("/api/books", request);

            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

            var problemDetails = await response.Content
                .ReadFromJsonAsync<ProblemDetails>();

            problemDetails.ShouldNotBeNull();
            problemDetails.Title.ShouldBe("Authors.NotFound");
            problemDetails.Detail.ShouldBe(
                $"Author with ID '{authorId}' was not found.");
        }
    }
}

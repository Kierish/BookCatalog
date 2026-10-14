using System.Net;
using System.Net.Http.Json;
using BookCatalog.Api.Features.Loans;
using BookCatalog.Api.Features.Loans.BorrowBook;
using BookCatalog.IntegrationTests.Features.Loans.Shared;
using BookCatalog.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace BookCatalog.IntegrationTests.Features.Loans
{
    [Collection(IntegrationTestCollection.Name)]
    public sealed class BorrowBookTests : IntegrationTestBase
    {
        public BorrowBookTests(IntegrationTestFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task BorrowBook_WhenBookIsAvailable_ShouldCreateActiveLoan()
        {
            var (author, book, user) = LoanTestData.Create();
            await SeedAsync(author, book, user);

            var request = new BorrowBookRequest(book.Id, user.Id);

            var response = await Client.PostAsJsonAsync("/api/loans", request);

            response.StatusCode.ShouldBe(HttpStatusCode.Created);

            var createdLoan = await response.Content
                .ReadFromJsonAsync<LoanResponse>();

            createdLoan.ShouldNotBeNull();
            createdLoan.Id.ShouldNotBe(Guid.Empty);
            createdLoan.BookId.ShouldBe(book.Id);
            createdLoan.UserId.ShouldBe(user.Id);
            createdLoan.BorrowedAt.ShouldNotBe(default);
            createdLoan.ReturnedAt.ShouldBeNull();

            await ExecuteDbContextAsync(async dbContext =>
            {
                var persistedLoan = await dbContext.Loans
                    .AsNoTracking()
                    .SingleAsync(loan => loan.Id == createdLoan.Id);

                var persistedUser = await dbContext.Users
                    .AsNoTracking()
                    .SingleAsync(databaseUser => databaseUser.Id == user.Id);

                persistedLoan.BookId.ShouldBe(book.Id);
                persistedLoan.UserId.ShouldBe(user.Id);
                persistedLoan.ReturnedAt.ShouldBeNull();
                persistedUser.ActiveLoanCount.ShouldBe(1);
            });
        }

        [Fact]
        public async Task BorrowBook_WhenBookIsAlreadyBorrowed_ShouldReturnConflict()
        {
            var (author, book, user) = LoanTestData.Create();
            await SeedAsync(author, book, user);

            var request = new BorrowBookRequest(book.Id, user.Id);

            var firstResponse = await Client.PostAsJsonAsync("/api/loans", request);
            var secondResponse = await Client.PostAsJsonAsync("/api/loans", request);

            firstResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
            secondResponse.StatusCode.ShouldBe(HttpStatusCode.Conflict);

            var problemDetails = await secondResponse.Content
                .ReadFromJsonAsync<ProblemDetails>();

            problemDetails.ShouldNotBeNull();
            problemDetails.Title.ShouldBe("Loans.BookAlreadyBorrowed");

            await ExecuteDbContextAsync(async dbContext =>
            {
                var activeLoanCount = await dbContext.Loans.CountAsync(loan =>
                    loan.BookId == book.Id && loan.ReturnedAt == null);

                var persistedUser = await dbContext.Users
                    .AsNoTracking()
                    .SingleAsync(databaseUser => databaseUser.Id == user.Id);

                activeLoanCount.ShouldBe(1);
                persistedUser.ActiveLoanCount.ShouldBe(1);
            });
        }

        [Fact]
        public async Task BorrowBook_WhenBookDoesNotExist_ShouldReturnNotFound()
        {
            var bookId = Guid.CreateVersion7();
            var request = new BorrowBookRequest(
                bookId,
                Guid.CreateVersion7());

            var response = await Client.PostAsJsonAsync("/api/loans", request);

            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

            var problemDetails = await response.Content
                .ReadFromJsonAsync<ProblemDetails>();

            problemDetails.ShouldNotBeNull();
            problemDetails.Title.ShouldBe("Books.NotFound");
            problemDetails.Detail.ShouldBe(
                $"Book with ID '{bookId}' was not found.");
        }

        [Fact]
        public async Task BorrowBook_WhenUserDoesNotExist_ShouldReturnNotFound()
        {
            var (author, book, _) = LoanTestData.Create();
            await SeedAsync(author, book);

            var userId = Guid.CreateVersion7();
            var request = new BorrowBookRequest(book.Id, userId);

            var response = await Client.PostAsJsonAsync("/api/loans", request);

            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

            var problemDetails = await response.Content
                .ReadFromJsonAsync<ProblemDetails>();

            problemDetails.ShouldNotBeNull();
            problemDetails.Title.ShouldBe("Users.NotFound");
            problemDetails.Detail.ShouldBe(
                $"User with ID '{userId}' was not found.");
        }

        private Task SeedAsync(params object[] entities)
        {
            return ExecuteDbContextAsync(async dbContext =>
            {
                dbContext.AddRange(entities);
                await dbContext.SaveChangesAsync();
            });
        }
    }
}

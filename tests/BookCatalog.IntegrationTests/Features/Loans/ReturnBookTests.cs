using System.Net;
using System.Net.Http.Json;
using BookCatalog.Api.Features.Loans;
using BookCatalog.Api.Features.Loans.BorrowBook;
using BookCatalog.Domain.Entities;
using BookCatalog.IntegrationTests.Features.Loans.Shared;
using BookCatalog.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace BookCatalog.IntegrationTests.Features.Loans
{
    [Collection(IntegrationTestCollection.Name)]
    public sealed class ReturnBookTests : IntegrationTestBase
    {
        public ReturnBookTests(IntegrationTestFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task ReturnBook_WhenLoanIsActive_ShouldCompleteLoan()
        {
            var (author, book, user) = LoanTestData.Create();
            await SeedAsync(author, book, user);

            var loan = await BorrowBookAsync(book.Id, user.Id);

            var response = await Client.PostAsync(
                $"/api/loans/{loan.Id}/return",
                content: null);

            response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

            await ExecuteDbContextAsync(async dbContext =>
            {
                var persistedLoan = await dbContext.Loans
                    .AsNoTracking()
                    .SingleAsync(databaseLoan => databaseLoan.Id == loan.Id);

                var persistedUser = await dbContext.Users
                    .AsNoTracking()
                    .SingleAsync(databaseUser => databaseUser.Id == user.Id);

                persistedLoan.ReturnedAt.ShouldNotBeNull();
                persistedLoan.ReturnedAt.Value.ShouldBeGreaterThanOrEqualTo(
                    persistedLoan.BorrowedAt);
                persistedUser.ActiveLoanCount.ShouldBe(0);
            });
        }

        [Fact]
        public async Task ReturnBook_WhenLoanWasAlreadyReturned_ShouldReturnConflict()
        {
            var (author, book, user) = LoanTestData.Create();
            await SeedAsync(author, book, user);

            var loan = await BorrowBookAsync(book.Id, user.Id);

            var firstResponse = await Client.PostAsync(
                $"/api/loans/{loan.Id}/return",
                content: null);

            var secondResponse = await Client.PostAsync(
                $"/api/loans/{loan.Id}/return",
                content: null);

            firstResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
            secondResponse.StatusCode.ShouldBe(HttpStatusCode.Conflict);

            var problemDetails = await secondResponse.Content
                .ReadFromJsonAsync<ProblemDetails>();

            problemDetails.ShouldNotBeNull();
            problemDetails.Title.ShouldBe("Loans.AlreadyReturned");

            await ExecuteDbContextAsync(async dbContext =>
            {
                var persistedUser = await dbContext.Users
                    .AsNoTracking()
                    .SingleAsync(databaseUser => databaseUser.Id == user.Id);

                persistedUser.ActiveLoanCount.ShouldBe(0);
            });
        }

        [Fact]
        public async Task ReturnBook_WhenLoanDoesNotExist_ShouldReturnNotFound()
        {
            var loanId = Guid.CreateVersion7();

            var response = await Client.PostAsync(
                $"/api/loans/{loanId}/return",
                content: null);

            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

            var problemDetails = await response.Content
                .ReadFromJsonAsync<ProblemDetails>();

            problemDetails.ShouldNotBeNull();
            problemDetails.Title.ShouldBe("Loans.NotFound");
            problemDetails.Detail.ShouldBe(
                $"Loan with ID '{loanId}' was not found.");
        }

        private async Task<LoanResponse> BorrowBookAsync(
            Guid bookId,
            Guid userId)
        {
            var request = new BorrowBookRequest(bookId, userId);
            var response = await Client.PostAsJsonAsync("/api/loans", request);

            response.StatusCode.ShouldBe(HttpStatusCode.Created);

            var loan = await response.Content.ReadFromJsonAsync<LoanResponse>();
            loan.ShouldNotBeNull();

            return loan;
        }

        private Task SeedAsync(Author author, Book book, User user)
        {
            return ExecuteDbContextAsync(async dbContext =>
            {
                dbContext.AddRange(author, book, user);
                await dbContext.SaveChangesAsync();
            });
        }
    }
}

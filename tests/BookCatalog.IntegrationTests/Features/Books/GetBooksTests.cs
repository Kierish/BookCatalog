using System.Net;
using BookCatalog.IntegrationTests.Infrastructure;
using Shouldly;

namespace BookCatalog.IntegrationTests.Features.Books
{
    [Collection(IntegrationTestCollection.Name)]
    public sealed class GetBooksTests : IntegrationTestBase
    {
        public GetBooksTests(IntegrationTestFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task GetBooks_WhenDatabaseIsEmpty_ShouldReturnOk()
        {
            var response = await Client.GetAsync("/api/books");

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }
}

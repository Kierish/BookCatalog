using BookCatalog.Infrastructure.Persistence;

namespace BookCatalog.IntegrationTests.Infrastructure
{
    public abstract class IntegrationTestBase : IAsyncLifetime
    {
        private readonly IntegrationTestFixture _fixture;

        protected IntegrationTestBase(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        protected HttpClient Client => _fixture.Client;

        protected Task ExecuteDbContextAsync(Func<BookCatalogDbContext, Task> action)
        {
            return _fixture.ExecuteDbContextAsync(action);
        }

        public Task InitializeAsync()
        {
            return _fixture.ResetDatabaseAsync();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}

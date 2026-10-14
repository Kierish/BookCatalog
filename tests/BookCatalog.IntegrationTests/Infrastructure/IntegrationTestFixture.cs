using BookCatalog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;

namespace BookCatalog.IntegrationTests.Infrastructure
{
    public sealed class IntegrationTestFixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgresContainer =
            new PostgreSqlBuilder("postgres:17-alpine")
                .WithDatabase("bookcatalog_tests")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

        private BookCatalogApiFactory? _application;
        private NpgsqlConnection? _databaseConnection;
        private Respawner? _respawner;

        public HttpClient Client { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            await _postgresContainer.StartAsync();

            _application = new BookCatalogApiFactory(
                _postgresContainer.GetConnectionString());

            await ApplyMigrationsAsync();

            _databaseConnection = new NpgsqlConnection(
                _postgresContainer.GetConnectionString());

            await _databaseConnection.OpenAsync();

            _respawner = await Respawner.CreateAsync(
                _databaseConnection,
                new RespawnerOptions
                {
                    DbAdapter = DbAdapter.Postgres,
                    SchemasToInclude = ["public"],
                    TablesToIgnore =
                    [
                        new Table("public", "__EFMigrationsHistory")
                    ]
                });

            Client = _application.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    BaseAddress = new Uri("https://localhost")
                });
        }

        public async Task ResetDatabaseAsync()
        {
            if (_respawner is null || _databaseConnection is null)
            {
                throw new InvalidOperationException(
                    "The integration test database is not initialized.");
            }

            await _respawner.ResetAsync(_databaseConnection);
        }

        public async Task ExecuteDbContextAsync(Func<BookCatalogDbContext, Task> action)
        {
            if (_application is null)
            {
                throw new InvalidOperationException(
                    "The test application is not initialized.");
            }

            await using var scope = _application.Services.CreateAsyncScope();

            var dbContext = scope.ServiceProvider
                .GetRequiredService<BookCatalogDbContext>();

            await action(dbContext);
        }

        private async Task ApplyMigrationsAsync()
        {
            if (_application is null)
            {
                throw new InvalidOperationException(
                    "The test application is not initialized.");
            }

            await using var scope = _application.Services.CreateAsyncScope();

            var dbContext = scope.ServiceProvider
                .GetRequiredService<BookCatalogDbContext>();

            await dbContext.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            Client?.Dispose();

            if (_application is not null)
            {
                await _application.DisposeAsync();
            }

            if (_databaseConnection is not null)
            {
                await _databaseConnection.DisposeAsync();
            }

            await _postgresContainer.DisposeAsync();
        }
    }
}

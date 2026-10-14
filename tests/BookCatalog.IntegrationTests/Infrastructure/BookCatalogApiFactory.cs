using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace BookCatalog.IntegrationTests.Infrastructure
{
    public sealed class BookCatalogApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _connectionString;

        public BookCatalogApiFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureHostConfiguration(configuration =>
            {
                configuration.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:BookCatalog"] = _connectionString
                    });
            });

            return base.CreateHost(builder);
        }
    }
}

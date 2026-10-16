using Microsoft.Extensions.Options;
using Npgsql;

namespace BookCatalog.Api.Configuration
{
    public sealed class DatabaseOptionsValidator : IValidateOptions<DatabaseOptions>
    {
        public ValidateOptionsResult Validate(string? name, DatabaseOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.BookCatalog))
            {
                return ValidateOptionsResult.Fail(
                    "Configuration value " +
                    "'ConnectionStrings:BookCatalog' is required.");
            }

            try
            {
                var connectionStringBuilder =
                    new NpgsqlConnectionStringBuilder(options.BookCatalog);

                if (string.IsNullOrWhiteSpace(connectionStringBuilder.Host) ||
                    string.IsNullOrWhiteSpace(connectionStringBuilder.Database) ||
                    string.IsNullOrWhiteSpace(connectionStringBuilder.Username))
                {
                    return ValidateOptionsResult.Fail(
                        "Configuration value " +
                        "'ConnectionStrings:BookCatalog' must contain " +
                        "Host, Database, and Username.");
                }
            }
            catch (ArgumentException)
            {
                return ValidateOptionsResult.Fail(
                    "Configuration value " +
                    "'ConnectionStrings:BookCatalog' must be a valid " +
                    "PostgreSQL connection string.");
            }

            return ValidateOptionsResult.Success;
        }
    }
}

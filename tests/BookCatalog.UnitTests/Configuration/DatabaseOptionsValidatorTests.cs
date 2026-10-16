using BookCatalog.Api.Configuration;
using Shouldly;

namespace BookCatalog.UnitTests.Configuration
{
    public sealed class DatabaseOptionsValidatorTests
    {
        private readonly DatabaseOptionsValidator _validator = new();

        [Fact]
        public void Validate_WhenConnectionStringIsValid_ShouldSucceed()
        {
            var options = new DatabaseOptions
            {
                BookCatalog =
                    "Host=localhost;Database=bookcatalog;Username=postgres"
            };

            var result = _validator.Validate(null, options);

            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public void Validate_WhenConnectionStringIsEmpty_ShouldFail()
        {
            var options = new DatabaseOptions();

            var result = _validator.Validate(null, options);

            result.Failed.ShouldBeTrue();
            result.Failures.ShouldContain(
                "Configuration value " +
                "'ConnectionStrings:BookCatalog' is required.");
        }

        [Fact]
        public void Validate_WhenConnectionStringIsMalformed_ShouldFail()
        {
            var options = new DatabaseOptions
            {
                BookCatalog = "not-a-connection-string"
            };

            var result = _validator.Validate(null, options);

            result.Failed.ShouldBeTrue();
            result.Failures.ShouldContain(
                "Configuration value " +
                "'ConnectionStrings:BookCatalog' must be a valid " +
                "PostgreSQL connection string.");
        }

        [Fact]
        public void Validate_WhenRequiredComponentsAreMissing_ShouldFail()
        {
            var options = new DatabaseOptions
            {
                BookCatalog = "Host=localhost"
            };

            var result = _validator.Validate(null, options);

            result.Failed.ShouldBeTrue();
            result.Failures.ShouldContain(
                "Configuration value " +
                "'ConnectionStrings:BookCatalog' must contain " +
                "Host, Database, and Username.");
        }
    }
}

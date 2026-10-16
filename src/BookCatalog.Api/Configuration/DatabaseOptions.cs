namespace BookCatalog.Api.Configuration
{
    public sealed class DatabaseOptions
    {
        public const string SectionName = "ConnectionStrings";

        public string BookCatalog { get; init; } = string.Empty;
    }
}

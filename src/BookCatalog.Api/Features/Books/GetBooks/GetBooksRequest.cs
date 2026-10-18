namespace BookCatalog.Api.Features.Books.GetBooks
{
    public sealed record GetBooksRequest(
        string? Search = null,
        Guid? AuthorId = null,
        int? PublicationYear = null,
        int PageNumber = 1,
        int PageSize = 10);
}

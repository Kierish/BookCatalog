namespace BookCatalog.Api.Features.Books.GetBooksList
{
    public sealed record GetBooksListRequest(
        string? Search = null,
        Guid? AuthorId = null,
        int? PublicationYear = null,
        int PageNumber = 1,
        int PageSize = 10);
}

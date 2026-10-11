namespace BookCatalog.Api.Features.Books.CreateBook
{
    public sealed record CreateBookRequest(
        string Title,
        Guid AuthorId,
        string? Isbn,
        int PublicationYear
    );
}

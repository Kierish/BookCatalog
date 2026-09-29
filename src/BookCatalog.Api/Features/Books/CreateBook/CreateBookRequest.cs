namespace BookCatalog.Api.Features.Books.CreateBook
{
    public sealed record CreateBookRequest(
        string Title,
        string Author,
        string? Isbn,
        int PublicationYear
    );
}

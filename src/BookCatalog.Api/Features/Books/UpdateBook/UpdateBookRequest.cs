namespace BookCatalog.Api.Features.Books.UpdateBook
{
    public sealed record UpdateBookRequest(
        string Title,
        string Author,
        string? Isbn,
        int PublicationYear
    );
}

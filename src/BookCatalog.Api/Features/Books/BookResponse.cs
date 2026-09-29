namespace BookCatalog.Features.Books
{
    public sealed record BookResponse(
        Guid Id,
        string Title,
        string Author,
        string? Isbn,
        int PublicationYear
    );
}

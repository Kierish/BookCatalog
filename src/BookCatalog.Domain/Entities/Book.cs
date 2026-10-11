namespace BookCatalog.Domain.Entities
{
    public sealed class Book
    {
        public Guid Id { get; init; } = Guid.CreateVersion7();

        public required string Title { get; set; }

        public string? Isbn { get; set; }

        public int PublicationYear { get; set; }

        public Guid AuthorId { get; set; }

        public Author Author { get; set; } = null!;

        public ICollection<Loan> Loans { get; set; } = [];
    }
}

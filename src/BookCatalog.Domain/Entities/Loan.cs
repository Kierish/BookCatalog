namespace BookCatalog.Domain.Entities
{
    public sealed class Loan
    {
        public Guid Id { get; init; } = Guid.CreateVersion7();

        public Guid BookId { get; set; }

        public Book Book { get; set; } = null!;

        public Guid UserId { get; set; }

        public User User { get; set; } = null!;

        public DateTimeOffset BorrowedAt { get; set; }

        public DateTimeOffset? ReturnedAt { get; set; }
    }
}

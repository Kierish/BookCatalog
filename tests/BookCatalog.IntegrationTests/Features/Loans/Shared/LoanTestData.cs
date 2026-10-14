using BookCatalog.Domain.Entities;

namespace BookCatalog.IntegrationTests.Features.Loans.Shared
{
    internal static class LoanTestData
    {
        public static (Author Author, Book Book, User User) Create()
        {
            var author = new Author
            {
                Name = "George Orwell"
            };

            var book = new Book
            {
                Title = "1984",
                AuthorId = author.Id,
                Isbn = "978-0451524935",
                PublicationYear = 1949
            };

            var user = new User
            {
                Name = "Alice Johnson"
            };

            return (author, book, user);
        }
    }
}

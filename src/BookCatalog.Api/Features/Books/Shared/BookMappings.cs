using BookCatalog.Api.Features.Books.CreateBook;
using BookCatalog.Api.Features.Books.UpdateBook;
using BookCatalog.Domain.Entities;

namespace BookCatalog.Api.Features.Books
{
    public static class BookMappings
    {
        public static BookResponse ToResponse(this Book book)
        {
            return new BookResponse(
                book.Id,
                book.Title,
                book.AuthorId,
                book.Author.Name,
                book.Isbn,
                book.PublicationYear
            );
        }

        public static Book ToEntity(this CreateBookRequest request)
        {
            return new Book
            {
                Title = request.Title,
                AuthorId = request.AuthorId,
                Isbn = request.Isbn,
                PublicationYear = request.PublicationYear
            };
        }

        public static Book ToEntity(this UpdateBookRequest request, Guid id)
        {
            return new Book
            {
                Id = id,
                Title = request.Title,
                AuthorId = request.AuthorId,
                Isbn = request.Isbn,
                PublicationYear = request.PublicationYear
            };
        }
    }
}

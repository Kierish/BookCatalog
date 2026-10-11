using BookCatalog.Domain.Interfaces;
using BookCatalog.Domain.Common.Results;
using BookCatalog.Domain.Common.Results.Errors;

namespace BookCatalog.Api.Features.Books.GetBookById
{
    public sealed class GetBookByIdHandler
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<GetBookByIdHandler> _logger;

        public GetBookByIdHandler(IBookRepository bookRepository, ILogger<GetBookByIdHandler> logger)
        {
            _bookRepository = bookRepository;
            _logger = logger;
        }

        public async Task<Result<BookResponse>> HandleAsync(Guid id)
        {
            var book = await _bookRepository.GetByIdAsync(id);

            if (book is null)
            {
                _logger.LogWarning("Book with ID {Id} was not found", id);
                return Result<BookResponse>.Failure(
                    BookErrors.NotFound(id));
            }

            return Result<BookResponse>.Success(book.ToResponse());
        }
    }
}

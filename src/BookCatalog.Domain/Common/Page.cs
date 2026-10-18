namespace BookCatalog.Domain.Common
{
    public sealed record Page<T>(
        IReadOnlyList<T> Items,
        int TotalCount,
        int PageNumber,
        int PageSize)
    {
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public Page<TResult> Map<TResult>(Func<T, TResult> mapper)
        {
            return new Page<TResult>(
                Items.Select(mapper).ToList(),
                TotalCount,
                PageNumber,
                PageSize
            );
        }
    }
}

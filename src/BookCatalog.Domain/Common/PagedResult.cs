namespace BookCatalog.Domain.Common
{
    public sealed record PagedResult<T>(
        IReadOnlyList<T> Items,
        int TotalCount,
        int PageNumber,
        int PageSize)
    {
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public PagedResult<TResult> Map<TResult>(Func<T, TResult> mapper)
        {
            return new PagedResult<TResult>(
                Items.Select(mapper).ToList(),
                TotalCount,
                PageNumber,
                PageSize
            );
        }
    }
}

namespace BookCatalog.Domain.Common
{
    public sealed record PagedResult<T>(
        IReadOnlyList<T> Items,
        int TotalCount,
        int Page,
        int PageSize)
    {
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;

        public PagedResult<TResult> Map<TResult>(Func<T, TResult> mapper)
        {
            return new PagedResult<TResult>(
                Items.Select(mapper).ToList(),
                TotalCount,
                Page,
                PageSize
            );
        }
    }
}

using BookCatalog.Domain.Common;
using Shouldly;

namespace BookCatalog.UnitTests.Domain.Common
{
    public sealed class PagedResultTests
    {
        #region TotalPages

        [Theory]
        [InlineData(0, 10, 0)]
        [InlineData(1, 10, 1)]
        [InlineData(10, 10, 1)]
        [InlineData(11, 10, 2)]
        [InlineData(25, 10, 3)]
        public void TotalPages_ShouldRoundUpToWholeNumberOfPages(
            int totalCount,
            int pageSize,
            int expectedTotalPages)
        {
            var page = CreatePage(totalCount, pageNumber: 1, pageSize: pageSize);

            page.TotalPages.ShouldBe(expectedTotalPages);
        }

        #endregion

        #region HasPreviousPage

        [Theory]
        [InlineData(1, false)]
        [InlineData(2, true)]
        public void HasPreviousPage_ShouldBeTrueOnlyAfterFirstPage(int pageNumber, bool expected)
        {
            var page = CreatePage(totalCount: 25, pageNumber: pageNumber, pageSize: 10);

            page.HasPreviousPage.ShouldBe(expected);
        }

        #endregion

        #region HasNextPage

        [Theory]
        [InlineData(25, 1, 10, true)]
        [InlineData(25, 2, 10, true)]
        [InlineData(25, 3, 10, false)]
        [InlineData(20, 2, 10, false)]
        [InlineData(25, 5, 10, false)]
        [InlineData(0, 1, 10, false)]
        public void HasNextPage_ShouldBeTrueOnlyWhenMorePagesExist(
            int totalCount,
            int pageNumber,
            int pageSize,
            bool expected)
        {
            var page = CreatePage(totalCount, pageNumber, pageSize);

            page.HasNextPage.ShouldBe(expected);
        }

        #endregion

        #region Map

        [Fact]
        public void Map_WhenItemsExist_ShouldApplyMapperToEveryItemInOrder()
        {
            var page = new PagedResult<int>([3, 1, 2], TotalCount: 3, PageNumber: 1, PageSize: 10);

            var mapped = page.Map(x => $"#{x}");

            mapped.Items.ShouldBe(new[] { "#3", "#1", "#2" });
        }

        [Fact]
        public void Map_ShouldPreservePaginationMetadata()
        {
            var page = new PagedResult<int>([1, 2], TotalCount: 25, PageNumber: 2, PageSize: 10);

            var mapped = page.Map(x => x.ToString());

            mapped.ShouldSatisfyAllConditions(
                () => mapped.TotalCount.ShouldBe(25),
                () => mapped.PageNumber.ShouldBe(2),
                () => mapped.PageSize.ShouldBe(10));
        }

        [Fact]
        public void Map_WhenItemsAreEmpty_ShouldReturnEmptyItems()
        {
            var page = CreatePage(totalCount: 0, pageNumber: 1, pageSize: 10);

            var mapped = page.Map(x => x.ToString());

            mapped.Items.ShouldBeEmpty();
        }

        #endregion

        #region Helpers

        private static PagedResult<int> CreatePage(int totalCount, int pageNumber, int pageSize)
            => new([], totalCount, pageNumber, pageSize);

        #endregion
    }
}

namespace SearchService.RequestHelpers
{
    public abstract class PaginationParam
    {
        public int PageSize { get; set; } = 4;
        public int PageNumber { get; set; } = 1;
    }
}
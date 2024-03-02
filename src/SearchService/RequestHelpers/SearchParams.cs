namespace SearchService.RequestHelpers
{
    public class SearchParams : PaginationParam
    {
        public string SearchTerm { get; set; }
        public string Seller { get; set; }
        public string Winner { get; set; }
        public string OrderBy { get; set; }
        public string FilterBy { get; set; }
    }
}
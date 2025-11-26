namespace Wanas.API.Responses
{
    public class ApiPagedResponse<T>
    {
        public bool Success { get; set; } = true;
        public IEnumerable<T> Data { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public ApiPagedResponse(IEnumerable<T> data, int totalCount, int page, int pageSize)
        {
            Data = data;
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
        }
    }
}

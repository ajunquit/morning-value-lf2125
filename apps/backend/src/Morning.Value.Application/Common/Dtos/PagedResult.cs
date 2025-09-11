namespace Morning.Value.Application.Common.Dtos
{
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = new List<T>();
        public int PageIndex { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public int TotalCount { get; init; } = 0;
        public int TotalPages => (int)System.Math.Ceiling((double)TotalCount / PageSize);
        public string? Query { get; init; }
        public string StatusFilter { get; init; } = "all"; // all|borrowed|returned
    }
}

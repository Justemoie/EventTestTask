namespace EventTestTask.Core.Models.Pagination;

public class PageResult<T>
{
    public List<T> Data { get; set; }
    public int TotalCount { get; set; }

    public PageResult(List<T> data, int totalCount)
    {
        Data = data;
        TotalCount = totalCount;
    }
}
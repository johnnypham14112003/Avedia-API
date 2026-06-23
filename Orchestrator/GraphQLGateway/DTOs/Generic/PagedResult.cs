namespace GraphQLGateway.DTOs.Generic;

public class PagedResult<T> where T : class
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPage { get; set; }
    public IEnumerable<T>? DataList { get; set; }
}
namespace BusinessLogic.DTOs.Generic;

public class PagingQueryRq<T>
{
    public string? Keyword { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public T? AdvanceInput {  get; set; }
}

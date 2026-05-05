namespace BusinessLogic.Models.Generic;

public class ApiResult<T>//Not delcare T as class for more purpose return like bool
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = null!;
    public T? Data { get; set; }
}

using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Models.Generic;

public class ApiResult<T>//Not delcare T as class for more purpose return like bool
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = null!;
    public T? Data { get; set; }

    /// <summary>
    ///     Return status code 200 (service run ok all, no error)
    ///     <![CDATA[
    ///     Example:
    ///         In Controller:
    ///             var result = await _service.DoSomethingAsync(request);
    ///             return StatusCode(result.StatusCode, result);
    ///             
    ///         In Service:
    ///             Task<ApiResult<Model>> DoSomethingAsync(...){
    ///                 ...return ApiResult<Model>.Ok(resultData)
    ///             }
    ///     ]]>
    /// </summary>
    /// <returns>ApiResult (a wrapper detail for api response in json body)</returns>
    public static ApiResult<T> Ok(T data, string message = "Success")
        => new() { StatusCode = 200, Message = message, Data = data };

    /// <summary>
    ///     Return status code 201 (creation protocol - service run ok all, no error)
    ///     <![CDATA[
    ///     Example:
    ///         In Controller:
    ///             var result = await _service.DoSomethingAsync(request);
    ///             return StatusCode(result.StatusCode, result);
    ///             
    ///         In Service:
    ///             Task<ApiResult<Model>> CreateSomethingAsync(...){
    ///                 ...return ApiResult<Model>.Created(resultData)
    ///             }
    ///     ]]>
    /// </summary>
    /// <returns>ApiResult (wrapper for json body response)</returns>
    public static ApiResult<T> Created(T data, string message = "Created successfully")
        => new() { StatusCode = StatusCodes.Status201Created, Message = message, Data = data };
    public static ApiResult<T> Failure(T data, string message = "Failed")
        => new() { StatusCode = StatusCodes.Status422UnprocessableEntity, Message = message, Data = data };
}

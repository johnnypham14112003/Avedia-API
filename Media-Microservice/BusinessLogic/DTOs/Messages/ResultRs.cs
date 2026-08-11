namespace BusinessLogic.DTOs.Messages;

public class ResultRs<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    /// <summary><![CDATA[
    /// + (200) Ok : The server process the request success.
    /// + (400) Bad Request : The server cannot or will not process the request due to an apparent client error.
    /// + (404) Not Found : The requested resource could not be found but may be available in the future.
    /// + (409) Conflict : The resource try to edit or create is duplicated or cannot be changed.
    /// + (422) Unprocessable Content : the request was well-formed but could not be processed.
    /// ]]></summary>
    public int HttpCode { get; set; } = 0;

    public static ResultRs<T> Ok(T? data) => new() { Success = true, Data = data, HttpCode = 200 };
    /// <summary>
    /// For response that have no data, only boolean as result
    /// </summary>
    public static ResultRs<bool> OkBool(bool result) => new() { Success = result, HttpCode = 200 };
    public static ResultRs<T> BadRequest(string message = "The request is invalid to handle!") => new() { Success = false, ErrorMessage = message , HttpCode = 400};
    public static ResultRs<T> NotFound(string message = "Not found the resource!") => new() { Success = true, ErrorMessage = message , HttpCode = 404};
    public static ResultRs<T> Conflict(string message = "The resource trying to handle is duplicated or cannot be changed") => new() { Success = false, ErrorMessage = message , HttpCode = 409};
    public static ResultRs<T> Failure(string message = "The request was correct, but the server could not save data!") => new() { Success = false, ErrorMessage = message , HttpCode = 422};
}

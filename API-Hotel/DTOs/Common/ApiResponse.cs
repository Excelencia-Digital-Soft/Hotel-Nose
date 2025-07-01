namespace hotel.DTOs.Common;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
    public string? Message { get; set; }

    public static ApiResponse<T> Success(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message,
        };
    }

    public static ApiResponse<T> Failure(string error, string? message = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Errors = new List<string> { error },
            Message = message,
        };
    }

    public static ApiResponse<T> Failure(List<string> errors, string? message = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Errors = errors,
            Message = message,
        };
    }
}

// Non-generic version for operations that don't return data
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Success(string? message = null)
    {
        return new ApiResponse { IsSuccess = true, Message = message };
    }

    public static ApiResponse Failure(string error, string? message = null)
    {
        return new ApiResponse
        {
            IsSuccess = false,
            Errors = new List<string> { error },
            Message = message,
        };
    }

    public static ApiResponse Failure(List<string> errors, string? message = null)
    {
        return new ApiResponse
        {
            IsSuccess = false,
            Errors = errors,
            Message = message,
        };
    }
}


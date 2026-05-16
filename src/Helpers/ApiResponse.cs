namespace backend.src.Helpers;

public sealed record PaginationMetadata(int Page, int Limit, long Total);

public sealed record ApiResponse<T>(
    bool Success,
    T? Data,
    string Message,
    PaginationMetadata? Pagination = null)
{
    public static ApiResponse<T> Ok(T? data, string message = "Success", PaginationMetadata? pagination = null)
    {
        return new ApiResponse<T>(true, data, message, pagination);
    }

    public static ApiResponse<T> Fail(string message)
    {
        return new ApiResponse<T>(false, default, message);
    }
}

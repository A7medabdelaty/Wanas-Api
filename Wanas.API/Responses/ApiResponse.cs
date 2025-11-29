namespace Wanas.API.Responses
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }

        public ApiResponse(object? data = null, string? message = null)
        {
            Success = true;
            Data = data;
            Message = message;
        }

        public static ApiResponse Ok(object? data = null, string? message = null)
            => new ApiResponse(data, message);

        public static ApiResponse Created(object? data, string? message = null)
            => new ApiResponse(data, message);
    }
}

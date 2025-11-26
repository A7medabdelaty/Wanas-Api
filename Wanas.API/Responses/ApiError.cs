namespace Wanas.API.Responses
{
    public class ApiError
    {
        public bool Success { get; set; } = false;
        public string ErrorCode { get; set; }
        public string? Message { get; set; }

        public ApiError(string errorCode, string? message = null)
        {
            ErrorCode = errorCode;
            Message = message;
        }
    }
}

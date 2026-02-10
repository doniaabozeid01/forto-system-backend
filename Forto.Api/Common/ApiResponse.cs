namespace Forto.Api.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; init; }
        public string Message { get; init; } = "";
        public T? Data { get; init; }
        public Dictionary<string, string[]>? Errors { get; init; }
        //public string? TraceId { get; init; }

        public static ApiResponse<T> Ok(T data, string message, string traceId)
            => new() { Success = true, Message = message, Data = data, Errors = null };

        public static ApiResponse<T> Created(T data, string message, string traceId)
            => new() { Success = true, Message = message, Data = data, Errors = null };

        public static ApiResponse<T> Fail(string message, Dictionary<string, string[]>? errors, string traceId)
            => new() { Success = false, Message = message, Data = default, Errors = errors };
        
    }
}
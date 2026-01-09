namespace Forto.Api.Common
{
    public class BusinessException : Exception
    {
        public int StatusCode { get; }
        public Dictionary<string, string[]>? Errors { get; }

        public BusinessException(string message, int statusCode = 400, Dictionary<string, string[]>? errors = null)
            : base(message)
        {
            StatusCode = statusCode;
            Errors = errors;
        }
    }
}

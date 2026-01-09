using Forto.Api.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected string TraceId => HttpContext.TraceIdentifier;

        protected IActionResult OkResponse<T>(T data, string message = "OK")
            => Ok(ApiResponse<T>.Ok(data, message, TraceId));

        protected IActionResult CreatedResponse<T>(T data, string message = "Created")
            => StatusCode(201, ApiResponse<T>.Created(data, message, TraceId));

        protected IActionResult FailResponse(string message, int statusCode = 400, Dictionary<string, string[]>? errors = null)
            => StatusCode(statusCode, ApiResponse<object>.Fail(message, errors, TraceId));
    }
}




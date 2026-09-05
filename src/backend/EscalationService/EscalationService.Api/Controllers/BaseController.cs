using EscalationPlatform.Shared.Result;
using Microsoft.AspNetCore.Mvc;

namespace EscalationService.Api.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected IActionResult OkOrError<T>(Result<T> result) =>
        result.IsSuccess ? Ok(result.Value) : HandleError(result.Error!);
    
    protected IActionResult NoContentOrError(Result result) =>
        result.IsSuccess ? NoContent() : HandleError(result.Error!);

    private IActionResult HandleError(Error error)
    {
        var statusCode = error.ErrorType switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.AccessUnAuthorized => StatusCodes.Status401Unauthorized,
            ErrorType.AccessForbidden => StatusCodes.Status403Forbidden,
            ErrorType.Failure => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };

        return Problem(statusCode: statusCode, title: error.Code, detail: error.Description,
            extensions: error.Details.Count > 0
                ? new Dictionary<string, object?> { ["errors"] = error.Details }
                : null);
    }
}

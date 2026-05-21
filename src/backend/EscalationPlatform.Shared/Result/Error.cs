namespace EscalationPlatform.Shared.Result;

public sealed class Error
{
    private const string CodeValidationFailed = "VALIDATION_FAILED";
    private const string CodeNotFound = "NOT_FOUND";
    private const string CodeDuplicate = "DUPLICATE";
    private const string CodeUnauthorized = "UNAUTHORIZED";
    private const string CodeForbidden = "FORBIDDEN";
    private const string CodeInternalServerError = "INTERNAL_SERVER_ERROR";

    public ErrorType ErrorType { get; }
    public string Code { get; }
    public string Description { get; }

    private Error(ErrorType errorType, string code, string description)
    {
        ErrorType = errorType;
        Code = code;
        Description = description;
    }
    
    public static Error ValidationFailed(string details)
        => new(ErrorType.Validation, CodeValidationFailed, $"Validation error: {details}");

    public static Error NotFound<T>(object id)
        => new(ErrorType.NotFound, CodeNotFound, $"{typeof(T).Name} with ID '{id}' not found");
    
    public static Error NotFound(string entityName, object id)
        => new(ErrorType.NotFound, CodeNotFound, $"{entityName} with ID '{id}' not found");

    public static Error Duplicate(string fieldName, string value)
        => new(ErrorType.Conflict, CodeDuplicate, $"{fieldName} '{value}' is already in use");

    public static Error Unauthorized(string? message = null)
        => new(ErrorType.AccessUnAuthorized, CodeUnauthorized, message ?? "Access denied");

    public static Error Forbidden(string? message = null)
        => new(ErrorType.AccessForbidden, CodeForbidden, message ?? "You do not have permission to perform this action");

    public static Error InternalServerError(string message)
        => new(ErrorType.Failure, CodeInternalServerError, $"Internal server error: {message}");
}
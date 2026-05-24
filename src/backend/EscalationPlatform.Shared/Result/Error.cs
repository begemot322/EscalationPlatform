namespace EscalationPlatform.Shared.Result;

public sealed class Error
{
    public ErrorType ErrorType { get; }
    public string Code { get; }
    public string Description { get; }
    public IReadOnlyList<string> Details { get; }

    private Error(ErrorType errorType, string code, string description, IReadOnlyList<string>? details = null)
    {
        ErrorType = errorType;
        Code = code;
        Description = description;
        Details = details ?? [];
    }
    
    public static Error ValidationFailed(IEnumerable<string> details)
        => new(ErrorType.Validation, ErrorCodes.Validation.Failed, "Validation failed", details.ToList());

    public static Error NotFound<T>(object id)
        => new(ErrorType.NotFound, ErrorCodes.Resource.NotFound, $"{typeof(T).Name} '{id}' not found");

    public static Error NotFound(string entityName, object id)
        => new(ErrorType.NotFound, ErrorCodes.Resource.NotFound, $"{entityName} '{id}' not found");

    public static Error Duplicate(string fieldName, string value)
        => new(ErrorType.Conflict, ErrorCodes.Resource.Duplicate, $"{fieldName} '{value}' is already in use");

    public static Error InvalidCredentials()
        => new(ErrorType.AccessUnAuthorized, ErrorCodes.Auth.InvalidCredentials, "Invalid email or password");

    public static Error EmailNotVerified()
        => new(ErrorType.AccessUnAuthorized, ErrorCodes.Auth.EmailNotVerified, "Email address is not verified");

    public static Error Unauthorized(string? message = null)
        => new(ErrorType.AccessUnAuthorized, ErrorCodes.Auth.Unauthorized, message ?? "Access denied");

    public static Error Forbidden(string? message = null)
        => new(ErrorType.AccessForbidden, ErrorCodes.Auth.Forbidden, message ?? "You do not have permission to perform this action");

    public static Error OtpInvalidOrExpired()
        => new(ErrorType.Validation, ErrorCodes.Otp.InvalidOrExpired, "OTP code is invalid or has expired");

    public static Error OtpResendTooSoon(int secondsLeft)
        => new(ErrorType.Validation, ErrorCodes.Otp.ResendTooSoon, $"Resend available in {secondsLeft} seconds");

    public static Error OtpAlreadyVerified()
        => new(ErrorType.Validation, ErrorCodes.Otp.AlreadyVerified, "Email address is already verified");

    public static Error InternalServerError(string message)
        => new(ErrorType.Failure, ErrorCodes.Server.InternalError, message);
}
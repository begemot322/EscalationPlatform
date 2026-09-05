namespace EscalationPlatform.Shared.Result;

public static class ErrorCodes
{
    public static class Validation
    {
        public const string Failed = "VALIDATION_FAILED";
    }

    public static class Auth
    {
        public const string InvalidCredentials = "INVALID_CREDENTIALS";
        public const string EmailNotVerified = "EMAIL_NOT_VERIFIED";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string Forbidden = "FORBIDDEN";
    }

    public static class Resource
    {
        public const string NotFound = "NOT_FOUND";
        public const string Duplicate = "DUPLICATE";
    }

    public static class Otp
    {
        public const string InvalidOrExpired = "OTP_INVALID_OR_EXPIRED";
        public const string ResendTooSoon = "OTP_RESEND_TOO_SOON";
        public const string AlreadyVerified = "EMAIL_ALREADY_VERIFIED";
    }

    public static class Server
    {
        public const string InternalError = "INTERNAL_SERVER_ERROR";
    }
}
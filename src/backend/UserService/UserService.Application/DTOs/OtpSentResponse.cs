namespace UserService.Application.DTOs;

public record OtpSentResponse(DateTime ExpiresAt, DateTime CanResendAt);

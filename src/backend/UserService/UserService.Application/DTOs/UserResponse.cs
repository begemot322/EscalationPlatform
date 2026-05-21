using UserService.Domain;

namespace UserService.Application.DTOs;

public record UserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string? ProfileImageFileName,
    UserRole Role,
    bool IsEmailVerified,
    DateTime CreatedAt);

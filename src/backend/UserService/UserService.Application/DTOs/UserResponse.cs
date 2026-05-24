using UserService.Domain.Enums;

namespace UserService.Application.DTOs;

public record UserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    UserRole Role,
    WorkType WorkType,
    string? ProfileImageFileName,
    bool IsEmailVerified,
    DateTime CreatedAt);

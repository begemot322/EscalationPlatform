using UserService.Domain;
using UserService.Domain.Enums;

namespace UserService.Application.DTOs;

public record RegisterUserDto(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string ConfirmPassword,
    UserRole Role);
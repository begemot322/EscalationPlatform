using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.Common.Interfaces.Identity;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.Infrastructure.Persistence;

public static class DataSeeder
{
    // Фиксированные GUIDs — используются также в EscalationService сидере
    public static readonly Guid User1 = new("00000000-0000-0000-0000-000000000001"); // Tech Lead / Admin
    public static readonly Guid User2 = new("00000000-0000-0000-0000-000000000002"); // Project Manager / Manager
    public static readonly Guid User3 = new("00000000-0000-0000-0000-000000000003"); // Backend Developer
    public static readonly Guid User4 = new("00000000-0000-0000-0000-000000000004"); // Frontend Developer
    public static readonly Guid User5 = new("00000000-0000-0000-0000-000000000005"); // QA Engineer
    public static readonly Guid User6 = new("00000000-0000-0000-0000-000000000006"); // DevOps

    private const string DefaultPassword = "Seed1234!";

    public static async Task SeedAsync(UserDbContext db, IPasswordHasher hasher, ILogger logger)
    {
        if (await db.Users.AnyAsync())
            return;

        var now = DateTime.UtcNow;
        var hash = hasher.Hash(DefaultPassword);

        var users = new List<User>
        {
            new()
            {
                Id = User1,
                FirstName = "Александр",
                LastName = "Петров",
                Email = "a.petrov@company.dev",
                PasswordHash = hash,
                Role = UserRole.Admin,
                WorkType = WorkType.TechLead,
                IsEmailVerified = true,
                CreatedAt = now,
            },
            new()
            {
                Id = User2,
                FirstName = "Мария",
                LastName = "Иванова",
                Email = "m.ivanova@company.dev",
                PasswordHash = hash,
                Role = UserRole.Manager,
                WorkType = WorkType.ProjectManager,
                IsEmailVerified = true,
                CreatedAt = now,
            },
            new()
            {
                Id = User3,
                FirstName = "Дмитрий",
                LastName = "Козлов",
                Email = "d.kozlov@company.dev",
                PasswordHash = hash,
                Role = UserRole.Employee,
                WorkType = WorkType.BackendDeveloper,
                IsEmailVerified = true,
                CreatedAt = now,
            },
            new()
            {
                Id = User4,
                FirstName = "Анна",
                LastName = "Смирнова",
                Email = "a.smirnova@company.dev",
                PasswordHash = hash,
                Role = UserRole.Employee,
                WorkType = WorkType.FrontendDeveloper,
                IsEmailVerified = true,
                CreatedAt = now,
            },
            new()
            {
                Id = User5,
                FirstName = "Сергей",
                LastName = "Новиков",
                Email = "s.novikov@company.dev",
                PasswordHash = hash,
                Role = UserRole.Employee,
                WorkType = WorkType.QaEngineer,
                IsEmailVerified = true,
                CreatedAt = now,
            },
            new()
            {
                Id = User6,
                FirstName = "Елена",
                LastName = "Фёдорова",
                Email = "e.fedorova@company.dev",
                PasswordHash = hash,
                Role = UserRole.Employee,
                WorkType = WorkType.DevOps,
                IsEmailVerified = true,
                CreatedAt = now,
            },
        };

        await db.Users.AddRangeAsync(users);
        await db.SaveChangesAsync();

        logger.LogInformation("UserService seed: {Count} users created (password: {Password})",
            users.Count, DefaultPassword);
    }
}

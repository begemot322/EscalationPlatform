using EscalationService.Domain.Entities;
using EscalationService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EscalationService.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(EscalationDbContext db, ILogger logger)
    {
        if (await db.Projects.AnyAsync())
            return;

        var now = DateTimeOffset.UtcNow;

        var projects = new List<Project>
        {
            new()
            {
                Name = "Platform Core",
                Description = "Основная платформа эскалаций",
                Code = "CORE",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Name = "Mobile App",
                Description = "Мобильное приложение",
                Code = "MOB",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Name = "Backend API",
                Description = "Серверная часть",
                Code = "API",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
        };

        await db.Projects.AddRangeAsync(projects);
        await db.SaveChangesAsync();

        var owner = new ProjectOwner
        {
            ProjectId = projects[0].Id,
            UserId = 1,
            Role = ProjectRole.Owner,
            AssignedAt = now
        };

        await db.ProjectOwners.AddAsync(owner);
        await db.SaveChangesAsync();

        logger.LogInformation("Seed data applied: {ProjectCount} projects", projects.Count);
    }
}
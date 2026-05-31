using EscalationService.Domain.Entities;
using EscalationService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EscalationService.Infrastructure.Persistence.DataSeeders;

/// <summary>
/// Тестовые данные для проверки SlaMonitorWorker.
///
/// Создает две эскалации с активными статусами:
/// 1.WARNING  - израсходовано ~87% SLA (должен повыситься приоритет)
/// 2.BREACH   -израсходовано ~400% SLA (должен повыситься приоритет + уведомление на email)
///
/// Повторный запуск ничего не добавит если данные уже есть.
/// </summary>
public static class SlaMonitorTestSeeder
{
    // GUIDs исполнителей из UserService DataSeeder
    private static readonly Guid U3 = new("00000000-0000-0000-0000-000000000003"); // Дмитрий Козлов  — Backend
    private static readonly Guid U6 = new("00000000-0000-0000-0000-000000000006"); // Елена Фёдорова  — DevOps

    private const string SlaPolicyName    = "[TEST] SLA Monitor Test Policy";
    private const string WarningEscName   = "[TEST] SLA Warning — 87%";
    private const string BreachEscName    = "[TEST] SLA Breach — 400%";

    public static async Task SeedAsync(EscalationDbContext db, ILogger logger)
    {
        // Если тестовые эскалации уже есть - выходим
        if (await db.Escalations.AnyAsync(e => e.Name == WarningEscName || e.Name == BreachEscName))
        {
            logger.LogInformation("SlaMonitorTestSeeder: test data already exists, skipping");
            return;
        }

        // Берём любой активный проект (должен быть создан основным DataSeeder)
        var project = await db.Projects.FirstOrDefaultAsync(p => p.IsActive);
        if (project is null)
        {
            logger.LogWarning("SlaMonitorTestSeeder: no active project found, skipping");
            return;
        }

        var now = DateTimeOffset.UtcNow;

        // SLA политика: время разрешения 60 минут
        var slaPolicy = await db.SlaPolicies.FirstOrDefaultAsync(p => p.Name == SlaPolicyName);
        if (slaPolicy is null)
        {
            slaPolicy = new SlaPolicy
            {
                Name = SlaPolicyName,
                Description = "Тестовая SLA политика для проверки SlaMonitorWorker (60 мин на разрешение)",
                MinPriorityLevel = EscalationPriority.Low,
                IsActive = true,
                ResponseTimeMinutes = 15,
                ResolutionTimeMinutes = 60,
                CreatedAt = now,
                UpdatedAt = now,
            };

            db.SlaPolicies.Add(slaPolicy);
            await db.SaveChangesAsync();
        }

        var escalations = new List<Escalation>
        {
            // ── WARNING: elapsed = 52 мин / 60 мин = ~87% ────────────────────
            // Ожидаемое поведение: повышение приоритета, без уведомления
            new()
            {
                Name = WarningEscName,
                Description = "Тестовая эскалация: SLA израсходован на ~87%. Ожидается повышение приоритета.",
                Status = EscalationStatus.InProgress,
                Priority = EscalationPriority.Low,
                Type = EscalationType.Incident,
                AuthorId = U3,
                SlaPolicyId = slaPolicy.Id,
                ProjectId = project.Id,
                CreatedAt = now.AddMinutes(-52),
                UpdatedAt = now.AddMinutes(-52),
                Participants =
                [
                    new EscalationParticipant
                    {
                        UserId = U3,
                        Role = ParticipantRole.Assignee,
                        AssignedAt = now.AddMinutes(-52)
                    }
                ]
            },

            // ── BREACH: elapsed = 240 мин / 60 мин = 400% ────────────────────
            // Ожидаемое поведение: повышение приоритета + публикация SlaBreached → email
            new()
            {
                Name = BreachEscName,
                Description = "Тестовая эскалация: SLA израсходован на ~400%. Ожидается повышение приоритета и отправка уведомления.",
                Status = EscalationStatus.New,
                Priority = EscalationPriority.Medium,
                Type = EscalationType.Incident,
                AuthorId = U6,
                SlaPolicyId = slaPolicy.Id,
                ProjectId = project.Id,
                CreatedAt = now.AddHours(-4),
                UpdatedAt = now.AddHours(-4),
                Participants =
                [
                    new EscalationParticipant
                    {
                        UserId = U6,
                        Role = ParticipantRole.Assignee,
                        AssignedAt = now.AddHours(-4)
                    }
                ]
            },
        };

        await db.Escalations.AddRangeAsync(escalations);
        await db.SaveChangesAsync();

        logger.LogInformation(
            "SlaMonitorTestSeeder: created 2 test escalations " +
            "(WARNING ~87% → id project '{Project}', BREACH ~400% → assignee U6)",
            project.Name);
    }
}

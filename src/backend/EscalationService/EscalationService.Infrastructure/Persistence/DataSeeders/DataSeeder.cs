using EscalationService.Domain.Entities;
using EscalationService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EscalationService.Infrastructure.Persistence.DataSeeders;

public static class DataSeeder
{
    // Те же GUIDs что и в UserService.DataSeeder
    static readonly Guid U1 = new("00000000-0000-0000-0000-000000000001"); // Александр Петров  — TechLead / Admin
    static readonly Guid U2 = new("00000000-0000-0000-0000-000000000002"); // Мария Иванова     — PM / Manager
    static readonly Guid U3 = new("00000000-0000-0000-0000-000000000003"); // Дмитрий Козлов    — Backend
    static readonly Guid U4 = new("00000000-0000-0000-0000-000000000004"); // Анна Смирнова     — Frontend
    static readonly Guid U5 = new("00000000-0000-0000-0000-000000000005"); // Сергей Новиков    — QA
    static readonly Guid U6 = new("00000000-0000-0000-0000-000000000006"); // Елена Фёдорова    — DevOps

    public static async Task SeedAsync(EscalationDbContext db, ILogger logger)
    {
        if (await db.Projects.AnyAsync())
            return;

        var now = DateTimeOffset.UtcNow;

        // ── SLA-политики ────────────────────────────────────────────────────
        var slaCritical = new SlaPolicy
        {
            Name = "Critical SLA",
            Description = "Для критических инцидентов: ответ за 30 мин, решение за 4 часа",
            MinPriorityLevel = EscalationPriority.Critical,
            IsActive = true,
            ResponseTimeMinutes = 30,
            ResolutionTimeMinutes = 240,
            CreatedAt = now,
            UpdatedAt = now,
        };
        var slaStandard = new SlaPolicy
        {
            Name = "Standard SLA",
            Description = "Стандартный уровень: ответ за 2 часа, решение за 24 часа",
            MinPriorityLevel = EscalationPriority.Medium,
            IsActive = true,
            ResponseTimeMinutes = 120,
            ResolutionTimeMinutes = 1440,
            CreatedAt = now,
            UpdatedAt = now,
        };
        var slaLow = new SlaPolicy
        {
            Name = "Low Priority SLA",
            Description = "Низкий приоритет: ответ за 8 часов, решение за 72 часа",
            MinPriorityLevel = EscalationPriority.Low,
            IsActive = true,
            ResponseTimeMinutes = 480,
            ResolutionTimeMinutes = 4320,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await db.SlaPolicies.AddRangeAsync(slaCritical, slaStandard, slaLow);
        await db.SaveChangesAsync();

        // ── Проекты ─────────────────────────────────────────────────────────
        var projects = new List<Project>
        {
            new()
            {
                Name = "Platform Core",
                Description = "Основная платформа эскалаций — маршрутизация, уведомления, аналитика",
                Code = "CORE",
                IsActive = true,
                CreatedAt = now.AddDays(-60),
                UpdatedAt = now,
                Owners =
                [
                    new ProjectOwner { UserId = U1, Role = ProjectRole.Owner,   AssignedAt = now.AddDays(-60) },
                    new ProjectOwner { UserId = U2, Role = ProjectRole.Manager,  AssignedAt = now.AddDays(-60) },
                ]
            },
            new()
            {
                Name = "Mobile App",
                Description = "iOS/Android приложение для команды поддержки",
                Code = "MOB",
                IsActive = true,
                CreatedAt = now.AddDays(-45),
                UpdatedAt = now,
                Owners =
                [
                    new ProjectOwner { UserId = U2, Role = ProjectRole.Owner,   AssignedAt = now.AddDays(-45) },
                    new ProjectOwner { UserId = U4, Role = ProjectRole.Manager,  AssignedAt = now.AddDays(-45) },
                ]
            },
            new()
            {
                Name = "Backend API",
                Description = "REST API — интеграции, webhook-и, публичный API v2",
                Code = "API",
                IsActive = true,
                CreatedAt = now.AddDays(-30),
                UpdatedAt = now,
                Owners =
                [
                    new ProjectOwner { UserId = U3, Role = ProjectRole.Owner,   AssignedAt = now.AddDays(-30) },
                    new ProjectOwner { UserId = U6, Role = ProjectRole.Manager,  AssignedAt = now.AddDays(-30) },
                ]
            },
            new()
            {
                Name = "Data Analytics",
                Description = "Дашборды, отчёты и BI-интеграции",
                Code = "DATA",
                IsActive = true,
                CreatedAt = now.AddDays(-20),
                UpdatedAt = now,
                Owners =
                [
                    new ProjectOwner { UserId = U1, Role = ProjectRole.Owner,   AssignedAt = now.AddDays(-20) },
                ]
            },
            new()
            {
                Name = "Infrastructure",
                Description = "DevOps, CI/CD, мониторинг и облачная инфраструктура",
                Code = "INFRA",
                IsActive = true,
                CreatedAt = now.AddDays(-15),
                UpdatedAt = now,
                Owners =
                [
                    new ProjectOwner { UserId = U6, Role = ProjectRole.Owner,   AssignedAt = now.AddDays(-15) },
                    new ProjectOwner { UserId = U1, Role = ProjectRole.Manager,  AssignedAt = now.AddDays(-15) },
                ]
            },
            new()
            {
                Name = "Legacy Portal",
                Description = "Устаревший клиентский портал — только поддержка, без новых фич",
                Code = "LGCY",
                IsActive = false,
                ArchivedAt = now.AddDays(-5),
                CreatedAt = now.AddDays(-120),
                UpdatedAt = now.AddDays(-5),
                Owners =
                [
                    new ProjectOwner { UserId = U2, Role = ProjectRole.Owner,   AssignedAt = now.AddDays(-120) },
                ]
            },
        };

        await db.Projects.AddRangeAsync(projects);
        await db.SaveChangesAsync();

        var core  = projects[0];
        var mob   = projects[1];
        var api   = projects[2];
        var data  = projects[3];
        var infra = projects[4];

        // ── Эскалации ────────────────────────────────────────────────────────
        var escalations = new List<Escalation>
        {
            // CORE — 4 эскалации
            new()
            {
                Name = "Уведомления не доставляются при высокой нагрузке",
                Description = "При нагрузке >5000 rps уведомления задерживаются на 10–15 минут. Воспроизводится стабильно.",
                Status = EscalationStatus.InProgress,
                Priority = EscalationPriority.Critical,
                Type = EscalationType.Incident,
                AuthorId = U2,
                SlaPolicyId = slaCritical.Id,
                ProjectId = core.Id,
                CreatedAt = now.AddDays(-3),
                UpdatedAt = now.AddDays(-1),
                Participants =
                [
                    new EscalationParticipant { UserId = U3, Role = ParticipantRole.Assignee, AssignedAt = now.AddDays(-3) },
                    new EscalationParticipant { UserId = U1, Role = ParticipantRole.Observer,  AssignedAt = now.AddDays(-3) },
                ]
            },
            new()
            {
                Name = "Маршрутизация эскалаций теряет правила при рестарте",
                Description = "После рестарта сервиса часть правил маршрутизации не восстанавливается из Redis.",
                Status = EscalationStatus.New,
                Priority = EscalationPriority.High,
                Type = EscalationType.Bug,
                AuthorId = U3,
                SlaPolicyId = slaStandard.Id,
                ProjectId = core.Id,
                CreatedAt = now.AddDays(-1),
                UpdatedAt = now.AddDays(-1),
                Participants =
                [
                    new EscalationParticipant { UserId = U3, Role = ParticipantRole.Assignee, AssignedAt = now.AddDays(-1) },
                ]
            },
            new()
            {
                Name = "Добавить поддержку webhook-интеграций",
                Description = "Клиенты запрашивают webhook для получения событий об изменении статуса эскалаций.",
                Status = EscalationStatus.WaitingForResponse,
                Priority = EscalationPriority.Medium,
                Type = EscalationType.FeatureRequest,
                AuthorId = U2,
                SlaPolicyId = slaStandard.Id,
                ProjectId = core.Id,
                CreatedAt = now.AddDays(-10),
                UpdatedAt = now.AddDays(-2),
                Participants =
                [
                    new EscalationParticipant { UserId = U3, Role = ParticipantRole.Assignee, AssignedAt = now.AddDays(-10) },
                    new EscalationParticipant { UserId = U4, Role = ParticipantRole.Assignee, AssignedAt = now.AddDays(-10) },
                ]
            },
            new()
            {
                Name = "Оптимизация индексов в таблице escalations",
                Description = "Запросы с фильтрацией по статусу и проекту выполняются медленно (>500ms) на объёме >1M записей.",
                Status = EscalationStatus.Resolved,
                Priority = EscalationPriority.Medium,
                Type = EscalationType.Incident,
                AuthorId = U1,
                SlaPolicyId = slaStandard.Id,
                ProjectId = core.Id,
                ResolvedAt = now.AddDays(-5),
                CreatedAt = now.AddDays(-14),
                UpdatedAt = now.AddDays(-5),
                Participants =
                [
                    new EscalationParticipant { UserId = U3, Role = ParticipantRole.Assignee, AssignedAt = now.AddDays(-14) },
                ]
            },

            // MOB — 3 эскалации
            new()
            {
                Name = "Краш приложения при открытии уведомления",
                Description = "NullPointerException в NotificationActivity при tap на пуш-уведомление. Воспроизводится на Android 13.",
                Status = EscalationStatus.InProgress,
                Priority = EscalationPriority.Blocker,
                Type = EscalationType.Bug,
                AuthorId = U5,
                SlaPolicyId = slaCritical.Id,
                ProjectId = mob.Id,
                CreatedAt = now.AddDays(-2),
                UpdatedAt = now,
                Participants =
                [
                    new EscalationParticipant { UserId = U4, Role = ParticipantRole.Assignee, AssignedAt = now.AddDays(-2) },
                    new EscalationParticipant { UserId = U5, Role = ParticipantRole.Observer,  AssignedAt = now.AddDays(-2) },
                ]
            },
            new()
            {
                Name = "Добавить тёмную тему",
                Description = "Пользователи просят поддержку system dark mode.",
                Status = EscalationStatus.New,
                Priority = EscalationPriority.Low,
                Type = EscalationType.FeatureRequest,
                AuthorId = U4,
                SlaPolicyId = slaLow.Id,
                ProjectId = mob.Id,
                CreatedAt = now.AddDays(-7),
                UpdatedAt = now.AddDays(-7),
            },
            new()
            {
                Name = "Авторизация через биометрию не работает на Pixel 8",
                Description = "BiometricPrompt возвращает ERROR_HW_UNAVAILABLE на некоторых устройствах Pixel 8.",
                Status = EscalationStatus.Closed,
                Priority = EscalationPriority.High,
                Type = EscalationType.Bug,
                AuthorId = U5,
                SlaPolicyId = slaStandard.Id,
                ProjectId = mob.Id,
                ResolvedAt = now.AddDays(-10),
                ClosedAt = now.AddDays(-8),
                CreatedAt = now.AddDays(-18),
                UpdatedAt = now.AddDays(-8),
                Participants =
                [
                    new EscalationParticipant { UserId = U4, Role = ParticipantRole.Assignee, AssignedAt = now.AddDays(-18) },
                ]
            },

            // API — 3 эскалации
            new()
            {
                Name = "Rate limiting не работает для OAuth токенов",
                Description = "Запросы с Bearer токеном обходят rate limiting. Проблема в middleware порядке.",
                Status = EscalationStatus.InProgress,
                Priority = EscalationPriority.High,
                Type = EscalationType.Bug,
                AuthorId = U3,
                SlaPolicyId = slaStandard.Id,
                ProjectId = api.Id,
                CreatedAt = now.AddDays(-4),
                UpdatedAt = now.AddDays(-1),
                Participants =
                [
                    new EscalationParticipant { UserId = U3, Role = ParticipantRole.Assignee, AssignedAt = now.AddDays(-4) },
                    new EscalationParticipant { UserId = U6, Role = ParticipantRole.Observer,  AssignedAt = now.AddDays(-4) },
                ]
            },
            new()
            {
                Name = "Документация Swagger не соответствует реальным ответам",
                Description = "Несколько эндпоинтов возвращают дополнительные поля которых нет в схеме.",
                Status = EscalationStatus.New,
                Priority = EscalationPriority.Low,
                Type = EscalationType.ServiceRequest,
                AuthorId = U4,
                SlaPolicyId = slaLow.Id,
                ProjectId = api.Id,
                CreatedAt = now.AddDays(-6),
                UpdatedAt = now.AddDays(-6),
            },
            new()
            {
                Name = "Версионирование API: переход на v2",
                Description = "Нужно спроектировать v2 с breaking changes: новые форматы дат, пагинация cursor-based.",
                Status = EscalationStatus.WaitingForResponse,
                Priority = EscalationPriority.Medium,
                Type = EscalationType.FeatureRequest,
                AuthorId = U1,
                SlaPolicyId = slaStandard.Id,
                ProjectId = api.Id,
                CreatedAt = now.AddDays(-12),
                UpdatedAt = now.AddDays(-3),
                Participants =
                [
                    new EscalationParticipant { UserId = U3, Role = ParticipantRole.Assignee,  AssignedAt = now.AddDays(-12) },
                    new EscalationParticipant { UserId = U2, Role = ParticipantRole.Approver,   AssignedAt = now.AddDays(-12) },
                ]
            },

            // DATA — 2 эскалации
            new()
            {
                Name = "Дашборд зависает при загрузке данных за 6+ месяцев",
                Description = "Запрос агрегации на большом диапазоне дат вызывает timeout. Нужна оптимизация или кэширование.",
                Status = EscalationStatus.New,
                Priority = EscalationPriority.Medium,
                Type = EscalationType.Incident,
                AuthorId = U2,
                SlaPolicyId = slaStandard.Id,
                ProjectId = data.Id,
                CreatedAt = now.AddDays(-5),
                UpdatedAt = now.AddDays(-5),
                Participants =
                [
                    new EscalationParticipant { UserId = U3, Role = ParticipantRole.Assignee, AssignedAt = now.AddDays(-5) },
                ]
            },
            new()
            {
                Name = "Экспорт в Excel не включает фильтры",
                Description = "При экспорте данных активные фильтры игнорируются — выгружается весь датасет.",
                Status = EscalationStatus.Resolved,
                Priority = EscalationPriority.Medium,
                Type = EscalationType.Bug,
                AuthorId = U5,
                SlaPolicyId = slaStandard.Id,
                ProjectId = data.Id,
                ResolvedAt = now.AddDays(-2),
                CreatedAt = now.AddDays(-9),
                UpdatedAt = now.AddDays(-2),
                Participants =
                [
                    new EscalationParticipant { UserId = U4, Role = ParticipantRole.Assignee, AssignedAt = now.AddDays(-9) },
                ]
            },

            // INFRA — 3 эскалации
            new()
            {
                Name = "Prometheus не собирает метрики с escalation-service",
                Description = "После обновления Helm chart метрики пропали из Grafana. Проблема в конфигурации scrape job.",
                Status = EscalationStatus.InProgress,
                Priority = EscalationPriority.High,
                Type = EscalationType.Incident,
                AuthorId = U6,
                SlaPolicyId = slaCritical.Id,
                ProjectId = infra.Id,
                CreatedAt = now.AddHours(-18),
                UpdatedAt = now.AddHours(-2),
                Participants =
                [
                    new EscalationParticipant { UserId = U6, Role = ParticipantRole.Assignee, AssignedAt = now.AddHours(-18) },
                    new EscalationParticipant { UserId = U1, Role = ParticipantRole.Observer,  AssignedAt = now.AddHours(-18) },
                ]
            },
            new()
            {
                Name = "Увеличить лимиты памяти для user-service",
                Description = "OOMKilled при пиковой нагрузке. Нужно поднять limits с 256Mi до 512Mi.",
                Status = EscalationStatus.Resolved,
                Priority = EscalationPriority.Critical,
                Type = EscalationType.ServiceRequest,
                AuthorId = U1,
                SlaPolicyId = slaCritical.Id,
                ProjectId = infra.Id,
                ResolvedAt = now.AddDays(-1),
                CreatedAt = now.AddDays(-2),
                UpdatedAt = now.AddDays(-1),
                Participants =
                [
                    new EscalationParticipant { UserId = U6, Role = ParticipantRole.Assignee, AssignedAt = now.AddDays(-2) },
                ]
            },
            new()
            {
                Name = "Настроить алерты на latency p99 > 1s",
                Description = "Нет алертинга на деградацию производительности. Нужны правила Alertmanager.",
                Status = EscalationStatus.New,
                Priority = EscalationPriority.Medium,
                Type = EscalationType.ServiceRequest,
                AuthorId = U2,
                SlaPolicyId = slaStandard.Id,
                ProjectId = infra.Id,
                CreatedAt = now.AddDays(-3),
                UpdatedAt = now.AddDays(-3),
                Participants =
                [
                    new EscalationParticipant { UserId = U6, Role = ParticipantRole.Assignee, AssignedAt = now.AddDays(-3) },
                ]
            },
        };

        await db.Escalations.AddRangeAsync(escalations);
        await db.SaveChangesAsync();

        logger.LogInformation(
            "Seed data applied: {ProjectCount} projects, {SlaCount} SLA policies, {EscCount} escalations",
            projects.Count, 3, escalations.Count);
    }
}

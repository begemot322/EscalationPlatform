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
                FirstResponseAt = now.AddDays(-17),
                ResolvedAt = now.AddDays(-10),
                ClosedAt = now.AddDays(-8),
                CreatedAt = now.AddDays(-18),
                UpdatedAt = now.AddDays(-8),
                Participants =
                [
                    new EscalationParticipant { UserId = U4, Role = ParticipantRole.Assignee, AssignedAt = now.AddDays(-18) },
                ],
                Metrics = new EscalationMetrics
                {
                    FirstResponseTimeMinutes = 60,   // 1 ч — хорошо (SLA 120 мин)
                    ResolutionTimeMinutes    = 11520, // 8 дней
                    TimeToCloseMinutes       = 14400, // 10 дней
                    SlaViolated = false,
                    TotalComments = 5,
                    TotalParticipants = 1,
                }
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

            // CORE — закрытые с метриками
            new()
            {
                Name = "Оптимизация индексов в таблице escalations",
                Description = "Запросы с фильтрацией по статусу и проекту выполняются медленно (>500ms) на объёме >1M записей.",
                Status = EscalationStatus.Closed,
                Priority = EscalationPriority.Medium,
                Type = EscalationType.Incident,
                AuthorId = U1,
                SlaPolicyId = slaStandard.Id,
                ProjectId = core.Id,
                FirstResponseAt = now.AddDays(-13).AddHours(3),
                ResolvedAt = now.AddDays(-5),
                ClosedAt = now.AddDays(-4),
                CreatedAt = now.AddDays(-14),
                UpdatedAt = now.AddDays(-4),
                Participants =
                [
                    new EscalationParticipant { UserId = U3, Role = ParticipantRole.Assignee, AssignedAt = now.AddDays(-14) },
                ],
                Metrics = new EscalationMetrics
                {
                    FirstResponseTimeMinutes = 180,  // 3 ч — в SLA (120 мин Standard — чуть превышено)
                    ResolutionTimeMinutes    = 12960, // 9 дней
                    TimeToCloseMinutes       = 14400, // 10 дней
                    SlaViolated = true,               // ответ чуть позже нормы
                    TotalComments = 8,
                    TotalParticipants = 1,
                }
            },
            new()
            {
                Name = "Утечка памяти в notification worker",
                Description = "Worker постепенно съедает память и перезапускается раз в 6 часов. RSS растёт линейно.",
                Status = EscalationStatus.Closed,
                Priority = EscalationPriority.High,
                Type = EscalationType.Bug,
                AuthorId = U2,
                SlaPolicyId = slaStandard.Id,
                ProjectId = core.Id,
                FirstResponseAt = now.AddDays(-21).AddMinutes(45),
                ResolvedAt = now.AddDays(-19),
                ClosedAt = now.AddDays(-18),
                CreatedAt = now.AddDays(-22),
                UpdatedAt = now.AddDays(-18),
                Participants =
                [
                    new EscalationParticipant { UserId = U3, Role = ParticipantRole.Assignee, AssignedAt = now.AddDays(-22) },
                    new EscalationParticipant { UserId = U1, Role = ParticipantRole.Observer,  AssignedAt = now.AddDays(-22) },
                ],
                Metrics = new EscalationMetrics
                {
                    FirstResponseTimeMinutes = 45,   // 45 мин — отлично
                    ResolutionTimeMinutes    = 4320, // 3 дня
                    TimeToCloseMinutes       = 5760, // 4 дня
                    SlaViolated = false,
                    TotalComments = 12,
                    TotalParticipants = 2,
                }
            },

            // MOB — закрытые с метриками
            new()
            {
                Name = "Crash при повороте экрана в чате",
                Description = "IllegalStateException в ChatFragment при смене ориентации. Воспроизводится на Android 12+.",
                Status = EscalationStatus.Closed,
                Priority = EscalationPriority.High,
                Type = EscalationType.Bug,
                AuthorId = U5,
                SlaPolicyId = slaStandard.Id,
                ProjectId = mob.Id,
                FirstResponseAt = now.AddDays(-29).AddMinutes(30),
                ResolvedAt = now.AddDays(-26),
                ClosedAt = now.AddDays(-25),
                CreatedAt = now.AddDays(-30),
                UpdatedAt = now.AddDays(-25),
                Participants =
                [
                    new EscalationParticipant { UserId = U4, Role = ParticipantRole.Assignee, AssignedAt = now.AddDays(-30) },
                ],
                Metrics = new EscalationMetrics
                {
                    FirstResponseTimeMinutes = 30,   // 30 мин — отлично
                    ResolutionTimeMinutes    = 5760, // 4 дня
                    TimeToCloseMinutes       = 7200, // 5 дней
                    SlaViolated = false,
                    TotalComments = 6,
                    TotalParticipants = 1,
                }
            },
            new()
            {
                Name = "Push-уведомления не приходят на iOS 17",
                Description = "После обновления ОС до iOS 17 часть пользователей перестала получать push. Токены регистрируются нормально.",
                Status = EscalationStatus.Closed,
                Priority = EscalationPriority.Critical,
                Type = EscalationType.Incident,
                AuthorId = U2,
                SlaPolicyId = slaCritical.Id,
                ProjectId = mob.Id,
                FirstResponseAt = now.AddDays(-35).AddMinutes(50),
                ResolvedAt = now.AddDays(-33),
                ClosedAt = now.AddDays(-32),
                CreatedAt = now.AddDays(-36),
                UpdatedAt = now.AddDays(-32),
                Participants =
                [
                    new EscalationParticipant { UserId = U4, Role = ParticipantRole.Assignee,  AssignedAt = now.AddDays(-36) },
                    new EscalationParticipant { UserId = U6, Role = ParticipantRole.Assignee,  AssignedAt = now.AddDays(-36) },
                    new EscalationParticipant { UserId = U1, Role = ParticipantRole.Observer,   AssignedAt = now.AddDays(-36) },
                ],
                Metrics = new EscalationMetrics
                {
                    FirstResponseTimeMinutes = 50,   // 50 мин — нарушение (SLA Critical = 30 мин)
                    ResolutionTimeMinutes    = 4320, // 3 дня
                    TimeToCloseMinutes       = 5760, // 4 дня
                    SlaViolated = true,
                    TotalComments = 15,
                    TotalParticipants = 3,
                }
            },

            // API — закрытые с метриками
            new()
            {
                Name = "Некорректный 500 при пустом теле запроса",
                Description = "POST /api/v1/escalation без тела возвращает 500 вместо 400. Нужна явная валидация.",
                Status = EscalationStatus.Closed,
                Priority = EscalationPriority.Medium,
                Type = EscalationType.Bug,
                AuthorId = U4,
                SlaPolicyId = slaStandard.Id,
                ProjectId = api.Id,
                FirstResponseAt = now.AddDays(-40).AddHours(1),
                ResolvedAt = now.AddDays(-38),
                ClosedAt = now.AddDays(-37),
                CreatedAt = now.AddDays(-41),
                UpdatedAt = now.AddDays(-37),
                Participants =
                [
                    new EscalationParticipant { UserId = U3, Role = ParticipantRole.Assignee, AssignedAt = now.AddDays(-41) },
                ],
                Metrics = new EscalationMetrics
                {
                    FirstResponseTimeMinutes = 60,   // 1 ч — в норме
                    ResolutionTimeMinutes    = 4320, // 3 дня
                    TimeToCloseMinutes       = 5760, // 4 дня
                    SlaViolated = false,
                    TotalComments = 4,
                    TotalParticipants = 1,
                }
            },

            // INFRA — закрытые с метриками
            new()
            {
                Name = "Диск на prod-db заполнен на 95%",
                Description = "Свободно меньше 5% — нужно срочно почистить старые WAL-логи или расширить volume.",
                Status = EscalationStatus.Closed,
                Priority = EscalationPriority.Critical,
                Type = EscalationType.Incident,
                AuthorId = U1,
                SlaPolicyId = slaCritical.Id,
                ProjectId = infra.Id,
                FirstResponseAt = now.AddDays(-25).AddMinutes(20),
                ResolvedAt = now.AddDays(-25),
                ClosedAt = now.AddDays(-24),
                CreatedAt = now.AddDays(-26),
                UpdatedAt = now.AddDays(-24),
                Participants =
                [
                    new EscalationParticipant { UserId = U6, Role = ParticipantRole.Assignee, AssignedAt = now.AddDays(-26) },
                ],
                Metrics = new EscalationMetrics
                {
                    FirstResponseTimeMinutes = 20,   // 20 мин — отлично (SLA 30 мин)
                    ResolutionTimeMinutes    = 1200, // 20 ч
                    TimeToCloseMinutes       = 2880, // 2 дня
                    SlaViolated = false,
                    TotalComments = 7,
                    TotalParticipants = 1,
                }
            },
            new()
            {
                Name = "CI pipeline падает на этапе docker build",
                Description = "После обновления base image alpine:3.19 сборка падает из-за несовместимости пакетов.",
                Status = EscalationStatus.Closed,
                Priority = EscalationPriority.High,
                Type = EscalationType.Bug,
                AuthorId = U3,
                SlaPolicyId = slaStandard.Id,
                ProjectId = infra.Id,
                FirstResponseAt = now.AddDays(-44).AddHours(4),
                ResolvedAt = now.AddDays(-43),
                ClosedAt = now.AddDays(-42),
                CreatedAt = now.AddDays(-45),
                UpdatedAt = now.AddDays(-42),
                Participants =
                [
                    new EscalationParticipant { UserId = U6, Role = ParticipantRole.Assignee, AssignedAt = now.AddDays(-45) },
                    new EscalationParticipant { UserId = U5, Role = ParticipantRole.Observer,  AssignedAt = now.AddDays(-45) },
                ],
                Metrics = new EscalationMetrics
                {
                    FirstResponseTimeMinutes = 240,  // 4 ч — нарушение (SLA 120 мин)
                    ResolutionTimeMinutes    = 2880, // 2 дня
                    TimeToCloseMinutes       = 4320, // 3 дня
                    SlaViolated = true,
                    TotalComments = 9,
                    TotalParticipants = 2,
                }
            },
            new()
            {
                Name = "Сертификат TLS истекает через 7 дней",
                Description = "Wildcard сертификат *.waveaccess.io истекает. Нужна ручная замена или настройка auto-renewal.",
                Status = EscalationStatus.Closed,
                Priority = EscalationPriority.Blocker,
                Type = EscalationType.ServiceRequest,
                AuthorId = U2,
                SlaPolicyId = slaCritical.Id,
                ProjectId = infra.Id,
                FirstResponseAt = now.AddDays(-50).AddMinutes(15),
                ResolvedAt = now.AddDays(-49),
                ClosedAt = now.AddDays(-48),
                CreatedAt = now.AddDays(-51),
                UpdatedAt = now.AddDays(-48),
                Participants =
                [
                    new EscalationParticipant { UserId = U6, Role = ParticipantRole.Assignee, AssignedAt = now.AddDays(-51) },
                    new EscalationParticipant { UserId = U1, Role = ParticipantRole.Observer,  AssignedAt = now.AddDays(-51) },
                ],
                Metrics = new EscalationMetrics
                {
                    FirstResponseTimeMinutes = 15,   // 15 мин — отлично
                    ResolutionTimeMinutes    = 1440, // 1 день
                    TimeToCloseMinutes       = 4320, // 3 дня
                    SlaViolated = false,
                    TotalComments = 6,
                    TotalParticipants = 2,
                }
            },

            // DATA — закрытые с метриками
            new()
            {
                Name = "BI-экспорт дублирует строки при джойне",
                Description = "Отчёт по активным эскалациям задваивает записи при наличии нескольких участников.",
                Status = EscalationStatus.Closed,
                Priority = EscalationPriority.Medium,
                Type = EscalationType.Bug,
                AuthorId = U5,
                SlaPolicyId = slaStandard.Id,
                ProjectId = data.Id,
                FirstResponseAt = now.AddDays(-55).AddHours(2),
                ResolvedAt = now.AddDays(-53),
                ClosedAt = now.AddDays(-52),
                CreatedAt = now.AddDays(-56),
                UpdatedAt = now.AddDays(-52),
                Participants =
                [
                    new EscalationParticipant { UserId = U5, Role = ParticipantRole.Assignee, AssignedAt = now.AddDays(-56) },
                ],
                Metrics = new EscalationMetrics
                {
                    FirstResponseTimeMinutes = 120,  // ровно SLA
                    ResolutionTimeMinutes    = 5760, // 4 дня
                    TimeToCloseMinutes       = 5760, // 4 дня
                    SlaViolated = false,
                    TotalComments = 5,
                    TotalParticipants = 1,
                }
            },
            new()
            {
                Name = "Нет пагинации на странице отчётов",
                Description = "При загрузке >500 записей страница отчётов зависает. Нужно добавить серверную пагинацию.",
                Status = EscalationStatus.Closed,
                Priority = EscalationPriority.Medium,
                Type = EscalationType.FeatureRequest,
                AuthorId = U1,
                SlaPolicyId = slaStandard.Id,
                ProjectId = data.Id,
                FirstResponseAt = now.AddDays(-58).AddHours(5),
                ResolvedAt = now.AddDays(-56),
                ClosedAt = now.AddDays(-55),
                CreatedAt = now.AddDays(-59),
                UpdatedAt = now.AddDays(-55),
                Participants =
                [
                    new EscalationParticipant { UserId = U5, Role = ParticipantRole.Assignee, AssignedAt = now.AddDays(-59) },
                    new EscalationParticipant { UserId = U3, Role = ParticipantRole.Observer,  AssignedAt = now.AddDays(-59) },
                ],
                Metrics = new EscalationMetrics
                {
                    FirstResponseTimeMinutes = 300,  // 5 ч — нарушение SLA (120 мин)
                    ResolutionTimeMinutes    = 4320, // 3 дня
                    TimeToCloseMinutes       = 5760, // 4 дня
                    SlaViolated = true,
                    TotalComments = 7,
                    TotalParticipants = 2,
                }
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

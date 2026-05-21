namespace UserService.Domain;

/// <summary>
/// Роли пользователей в системе.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Технический лидер команды.
    /// </summary>
    TechLead = 1,

    /// <summary>
    /// Менеджер проекта.
    /// </summary>
    ProjectManager = 2,

    /// <summary>
    /// Backend разработчик.
    /// </summary>
    BackendDeveloper = 3,

    /// <summary>
    /// Frontend разработчик.
    /// </summary>
    FrontendDeveloper = 4,

    /// <summary>
    /// QA инженер.
    /// </summary>
    QaEngineer = 5,

    /// <summary>
    /// DevOps инженер.
    /// </summary>
    DevOps = 6,

    /// <summary>
    /// Дизайнер.
    /// </summary>
    Designer = 7
}
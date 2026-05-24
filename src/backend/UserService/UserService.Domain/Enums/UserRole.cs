namespace UserService.Domain.Enums;

public enum UserRole
{
    /// <summary>
    /// Обычный сотрудник. Может создавать и просматривать свои эскалации.
    /// </summary>
    Employee = 1,

    /// <summary>
    /// Менеджер. Может управлять эскалациями своей команды, назначать ответственных.
    /// </summary>
    Manager = 2,

    /// <summary>
    /// Администратор. Полный доступ к системе, управление пользователями.
    /// </summary>
    Admin = 3
}
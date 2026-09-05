namespace EscalationPlatform.Database.PgSql;

/// <summary>
/// Конфигурация базы данных
/// </summary>
public class DatabaseConfiguration
{
    /// <summary>
    /// Количество повторных попыток при возникновении ошибки базы данных, по умолчанию 0 (отключено)
    /// </summary>
    /// <remarks>
    /// Установите значение 0 для отключения повторных попыток
    /// </remarks>
    public int ErrorRetryCount { get; init; }
    
    /// <summary>
    /// Максимальная задержка в секундах перед повторной попыткой выполнения операции после ошибки. По умолчанию 30.
    /// </summary>
    public int ErrorRetryDelay { get; init; } = 30;
    
    /// <summary>
    /// Таймаут выполнения команд, по умолчанию 60 сек.
    /// </summary>
    public int? CommandTimeout { get; init; }
    
    /// <summary>
    /// Установите true для логирования чувствительных данных и полных SQL команд в консоль. По умолчанию false.
    /// </summary>
    public bool LogSensitiveData { get; init; }
    
    /// <summary>
    /// Определяет размер пула подключений к базе данных, по умолчанию 32
    /// </summary>
    public int PoolSize { get; init; } = 32;
}
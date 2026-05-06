using EscalationService.Domain.Common;

namespace EscalationService.Domain.Entities;

/// <summary>
/// Вложение (файл), связанное с эскалацией.
/// Содержит метаданные файла и путь к его хранению.
/// </summary>
public sealed class Attachment : BaseEntity
{
    /// <summary>
    /// Имя файла.
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    /// Путь к файлу в хранилище (MinIO/S3).
    /// </summary>
    public string StoragePath { get; set; } = null!;

    /// <summary>
    /// Идентификатор эскалации, к которой относится вложение.
    /// </summary>
    public long EscalationId { get; set; }
    
    /// <summary>
    /// Навигационное поле.
    /// Эскалация.
    /// </summary>
    public Escalation Escalation { get; set; } = null!;
}
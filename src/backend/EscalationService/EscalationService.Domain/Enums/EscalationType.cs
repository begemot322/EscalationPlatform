namespace EscalationService.Domain.Enums;

/// <summary>
/// Тип эскалации, определяющий её природу и категорию.
/// </summary>
public enum EscalationType
{
    /// <summary>Инцидент, сбой в работе системы или услуги.</summary>
    Incident = 1,

    /// <summary>Запрос на обслуживание или предоставление услуги.</summary>
    ServiceRequest = 2,

    /// <summary>Сообщение об ошибке в программном обеспечении.</summary>
    Bug = 3,

    /// <summary>Запрос на новую функциональность.</summary>
    FeatureRequest = 4
}

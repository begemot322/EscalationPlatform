namespace EscalationService.Domain.Enums;

/// <summary>
/// Роль пользователя в проекте.
/// </summary>
public enum ProjectRole
{
    /// <summary>Владелец проекта, имеет полные права управления.</summary>
    Owner = 1,

    /// <summary>Менеджер проекта, управляет задачами и участниками.</summary>
    Manager = 2,
}
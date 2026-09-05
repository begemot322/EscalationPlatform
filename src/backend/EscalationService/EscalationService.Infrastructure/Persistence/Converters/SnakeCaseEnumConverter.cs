using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EscalationService.Infrastructure.Persistence.Converters;

/// <summary>
/// Конвертер Enum в snake_case для PostgreSQL
/// </summary>
public class SnakeCaseEnumConverter<TEnum> : ValueConverter<TEnum, string>
    where TEnum : struct, Enum
{
    public SnakeCaseEnumConverter()
        : base(
            v => ToSnakeCase(v.ToString()),
            v => Enum.Parse<TEnum>(FromSnakeCase(v), true))
    {
    }

    private static string ToSnakeCase(string value)
    {
        return Regex
            .Replace(value, "([a-z0-9])([A-Z])", "$1_$2")
            .ToLowerInvariant();
    }

    private static string FromSnakeCase(string value)
    {
        return string.Concat(
            value.Split('_')
                .Select(x => char.ToUpper(x[0]) + x[1..]));
    }
}

using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace UserService.Infrastructure.Persistence.Converters;

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
        => Regex.Replace(value, "([a-z0-9])([A-Z])", "$1_$2").ToLowerInvariant();

    private static string FromSnakeCase(string value)
        => string.Concat(value.Split('_').Select(x => char.ToUpper(x[0]) + x[1..]));
}

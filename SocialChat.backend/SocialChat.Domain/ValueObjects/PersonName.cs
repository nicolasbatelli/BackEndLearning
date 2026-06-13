using System.Text.RegularExpressions;
using SocialChat.Domain.Common;

namespace SocialChat.Domain.ValueObjects;

public sealed class PersonName : ValueObject
{
    private static readonly Regex Pattern = new(@"^[a-zA-Z\s'-]+$", RegexOptions.Compiled);

    public string Value { get; }

    private PersonName(string value) => Value = value;

    public static PersonName Create(string value, string fieldName, bool required = true)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (required)
            {
                throw new DomainException($"{fieldName} is required.");
            }

            return new PersonName(string.Empty);
        }

        var trimmed = value.Trim();
        if (!Pattern.IsMatch(trimmed))
        {
            throw new DomainException($"{fieldName} may contain only letters.");
        }

        return new PersonName(trimmed);
    }

    public static PersonName? CreateOptional(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Create(value, fieldName, required: false);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}

using System.Text.RegularExpressions;
using SocialChat.Domain.Common;

namespace SocialChat.Domain.ValueObjects;

public sealed class Username : ValueObject
{
    private static readonly Regex Pattern = new(@"^[a-zA-Z0-9_]+$", RegexOptions.Compiled);

    public string Value { get; }

    private Username(string value) => Value = value;

    public static Username Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Username is required.");
        }

        var trimmed = value.Trim();
        if (trimmed.Length < 3 || trimmed.Length > 30)
        {
            throw new DomainException("Username must be between 3 and 30 characters.");
        }

        if (!Pattern.IsMatch(trimmed))
        {
            throw new DomainException("Username may contain only letters, numbers, and underscores.");
        }

        return new Username(trimmed);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}

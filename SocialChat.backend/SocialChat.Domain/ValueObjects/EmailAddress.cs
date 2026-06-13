using System.Text.RegularExpressions;
using SocialChat.Domain.Common;

namespace SocialChat.Domain.ValueObjects;

public sealed class EmailAddress : ValueObject
{
    private static readonly Regex Pattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private EmailAddress(string value) => Value = value;

    public static EmailAddress Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Email is required.");
        }

        var trimmed = value.Trim().ToLowerInvariant();
        if (!Pattern.IsMatch(trimmed))
        {
            throw new DomainException("Email format is invalid.");
        }

        return new EmailAddress(trimmed);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}

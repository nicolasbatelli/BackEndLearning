using System.Text.RegularExpressions;
using SocialChat.Domain.Common;

namespace SocialChat.Domain.ValueObjects;

public sealed class Password : ValueObject
{
    private static readonly Regex Uppercase = new("[A-Z]", RegexOptions.Compiled);
    private static readonly Regex Lowercase = new("[a-z]", RegexOptions.Compiled);
    private static readonly Regex Digit = new("[0-9]", RegexOptions.Compiled);
    private static readonly Regex Special = new(@"[^a-zA-Z0-9]", RegexOptions.Compiled);

    public string Value { get; }

    private Password(string value) => Value = value;

    public static Password Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Password is required.");
        }

        if (value.Length < 8)
        {
            throw new DomainException("Password must be at least 8 characters long.");
        }

        if (!Uppercase.IsMatch(value))
        {
            throw new DomainException("Password must contain at least one uppercase letter.");
        }

        if (!Lowercase.IsMatch(value))
        {
            throw new DomainException("Password must contain at least one lowercase letter.");
        }

        if (!Digit.IsMatch(value))
        {
            throw new DomainException("Password must contain at least one digit.");
        }

        if (!Special.IsMatch(value))
        {
            throw new DomainException("Password must contain at least one special character.");
        }

        return new Password(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}

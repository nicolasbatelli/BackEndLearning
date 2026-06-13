namespace SocialChat.Application.Common;

public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}

public class NotFoundException : BusinessRuleException
{
    public NotFoundException(string message) : base(message)
    {
    }
}

public class RequestValidationException : BusinessRuleException
{
    public IDictionary<string, string[]> Errors { get; }

    public RequestValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation failures have occurred.")
    {
        Errors = errors;
    }
}

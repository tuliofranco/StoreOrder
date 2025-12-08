namespace Order.Core.Application.Common.Exceptions;

public class OrderValidationException : Exception
{
    public IEnumerable<string> Errors { get; }

    public OrderValidationException(IEnumerable<string> errors)
        : base(string.Join("; ", errors))
    {
        Errors = errors;
    }
}

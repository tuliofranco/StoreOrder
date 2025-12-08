namespace Order.Core.Application.Common.Exceptions;

public class OrderNotFoundException : Exception
{
    public OrderNotFoundException(string message) : base(message) { }
}

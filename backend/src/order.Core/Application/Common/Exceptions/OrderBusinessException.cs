namespace Order.Core.Application.Common.Exceptions;

public class OrderBusinessException : Exception
{
    public OrderBusinessException(string message) : base(message) { }
}

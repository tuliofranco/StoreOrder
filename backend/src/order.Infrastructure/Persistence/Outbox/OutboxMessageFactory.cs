using System.Text.Json;
using System.Text.Json.Serialization;
using Order.Core.Domain.Common;
using Order.Core.Domain.Orders.Events;

namespace Order.Infrastructure.Persistence.Outbox;

public static class OutboxMessageFactory
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public static OutboxMessage FromDomainEvent(IDomainEvent domainEvent)
    {
        var type = domainEvent.GetType();

        object payloadObject = domainEvent;

        if (domainEvent is OrderCreatedDomainEvent e)
        {
            payloadObject = new
            {
                orderId = e.OrderId,
                orderNumber = e.OrderNumber,
                occurredOn = e.OccurredOn
            };
        }
    
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOn = domainEvent.OccurredOn,
            Type =  type.AssemblyQualifiedName!,
            Payload = JsonSerializer.Serialize(payloadObject, _jsonOptions),
            Processed = false
        };
    }
}



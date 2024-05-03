using Confluent.Kafka;

using MediatR;

using Microsoft.Extensions.Logging;

using System.Text;
using System.Text.Json;

namespace KafkaResilientEvent;

public class ConsumerHandler
{
    private readonly ILogger<ConsumerHandler> _logger;
    private readonly IMediator _mediator;
    private readonly ConsumerConfig _consumerConfig;
    private readonly Dictionary<string, KeyValuePair<Type, Type>> _eventTypes;

    public ConsumerHandler(
        ILogger<ConsumerHandler> logger,
        IMediator mediator,
        ConsumerConfig consumerConfig,
        Dictionary<string, KeyValuePair<Type, Type>> eventTypes)
    {
        _logger = logger;
        _mediator = mediator;
        _consumerConfig = consumerConfig;
        _eventTypes = eventTypes;
    }

    public async Task ConsumeAsync(string topic, CancellationToken cancellationToken)
    {
        using var consumer = new ConsumerBuilder<string, string>(_consumerConfig).Build();
        consumer.Subscribe(topic);
        _logger.LogInformation("Subscribed to topic: {Topic}", topic);

        while (!cancellationToken.IsCancellationRequested)
        {
            ConsumeResult<string, string>? consumerResult;
            try
            {
                consumerResult = consumer.Consume(cancellationToken);
            }
            catch (ConsumeException e)
            {
                _logger.LogError("Consume error: {Reason}. Skipping commit.", e.Error.Reason);
                continue;
            }

            if (consumerResult.IsPartitionEOF)
            {
                continue; // Skip end of partition events
            }

            try
            {
                var eventTypeStr = new ConsumeContext<string, string>(consumerResult.Message).GetEventType();
                _eventTypes.TryGetValue(eventTypeStr, out var type);

                object key = consumerResult.Message.Key;

                if (type.Key != typeof(Ignore) && type.Key != typeof(Null))
                {
                    key = JsonSerializer.Deserialize(consumerResult.Message.Key, type.Value);
                }

                var payload = JsonSerializer.Deserialize(consumerResult.Message.Value, type.Value);
                var message = new ConsumeContext<object, object>(key, payload, consumerResult.Headers);
                await _mediator.Send(message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while deserialing: {Message}", ex.Message);
                continue;
            }

            try
            {
                consumer.Commit(consumerResult);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to commit message: {Message}", ex.Message);
                continue;
            }
        }
    }
}

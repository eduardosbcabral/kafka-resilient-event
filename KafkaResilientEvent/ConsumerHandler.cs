using Confluent.Kafka;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Reflection;
using System.Text.Json;

namespace KafkaResilientEvent;

public class ConsumerHandler
{
    private readonly ILogger<ConsumerHandler> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConsumerConfig _consumerConfig;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly IDictionary<string, Type> _eventTypes;

    public ConsumerHandler(
        ILogger<ConsumerHandler> logger,
        IServiceProvider serviceProvider,
        ConsumerConfig consumerConfig,
        IDictionary<string, Type> eventTypes,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _consumerConfig = consumerConfig;
        _eventTypes = eventTypes;
        _jsonSerializerOptions = jsonSerializerOptions ?? new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        };
    }

    public async Task ConsumeAsync(string topic, CancellationToken cancellationToken)
    {
        using var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
        consumer.Subscribe(topic);
        _logger.LogInformation("Subscribed to topic: {Topic}", topic);

        while (!cancellationToken.IsCancellationRequested)
        {
            ConsumeResult<Ignore, string>? consumerResult;
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
                var eventTypeStr = ConsumeContext<IMessage>.GetEventType(consumerResult.Message.Headers);
                _eventTypes.TryGetValue(eventTypeStr, out var eventType);

                var payload = (IMessage)(JsonSerializer.Deserialize(consumerResult.Message.Value, eventType, _jsonSerializerOptions) ?? throw new Exception(eventTypeStr));
                object context = Activator.CreateInstance(typeof(ConsumeContext<>).MakeGenericType(eventType), consumerResult.Message.Key, payload);
                var eventConsumer = _serviceProvider.GetRequiredKeyedService(typeof(IConsumer<>).MakeGenericType([eventType]), $"Consumer.{eventTypeStr}");
                MethodInfo consumeMethod = eventConsumer.GetType().GetMethod("Consume");
                var task = (Task)consumeMethod.Invoke(eventConsumer, [context, cancellationToken]);
                await task.ConfigureAwait(false);
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

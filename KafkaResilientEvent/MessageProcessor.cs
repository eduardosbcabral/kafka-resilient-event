using Confluent.Kafka;

namespace KafkaResilientEvent;

public class SimpleMessageProcessor : IMessageProcessor
{
    private readonly ILogger<SimpleMessageProcessor> _logger;

    public SimpleMessageProcessor(ILogger<SimpleMessageProcessor> logger)
    {
        _logger = logger;
    }

    public async Task ProcessMessageAsync(Message<Ignore, string> message, CancellationToken cancellationToken)
    {
        // Simulate some asynchronous work
        await Task.Delay(100, cancellationToken);
        _logger.LogInformation($"Processing message: {message}");

        var payload = message.Value;
        // Determine success or failure based on some condition in the message
        if (payload.Contains("fail"))
        {
            throw new InvalidOperationException("Failed to process message.");
        }
    }
}

public interface IMessageProcessor
{
    Task ProcessMessageAsync(Message<Ignore, string> message, CancellationToken cancellationToken);
}
using Confluent.Kafka;

using Microsoft.Extensions.Logging;

namespace KafkaResilientEvent;

public class SimpleMessageChecker : IMessageChecker
{
    private readonly ILogger<SimpleMessageChecker> _logger;

    public SimpleMessageChecker(ILogger<SimpleMessageChecker> logger)
    {
        _logger = logger;
    }

    public async Task<bool> CanProcessMessageAsync(Message<Ignore, string> message, CancellationToken cancellationToken)
    {
        var payload = message.Value;
        // Example check: Process only messages containing "important"
        var canProcess = payload.Contains("important");
        if (!canProcess)
        {
            _logger.LogInformation($"Message skipped: {payload}");
        }
        return canProcess;
    }
}

public interface IMessageChecker
{
    Task<bool> CanProcessMessageAsync(Message<Ignore, string> message, CancellationToken cancellationToken);
}
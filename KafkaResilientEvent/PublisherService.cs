using Confluent.Kafka;

using System.Text;

public class PublisherService
{
    private readonly IProducer<Null, string> _producer;
    private readonly ILogger<PublisherService> _logger;

    public PublisherService(IProducer<Null, string> producer, ILogger<PublisherService> logger)
    {
        _producer = producer;
        _logger = logger;
    }

    public async Task PublishMessageAsync(string topic, Message<Ignore, string> message, int retryCount, CancellationToken cancellationToken)
    {
        var headers = new Headers { new Header("retry-count", Encoding.UTF8.GetBytes(retryCount.ToString())) };

        await _producer.ProduceAsync(topic, new Message<Null, string> { Value = message.Value, Headers = headers }, cancellationToken);
        _logger.LogInformation($"Message published to {topic} with retry count: {retryCount}");
    }
}

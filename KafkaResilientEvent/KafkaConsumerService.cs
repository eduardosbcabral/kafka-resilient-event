using Confluent.Kafka;

using KafkaResilientEvent;

using System.Text;

public class KafkaConsumerService
{
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly ConsumerConfig _consumerConfig;
    private readonly RetryService _retryService;
    private readonly IMessageChecker _messageChecker;
    private readonly IMessageProcessor _messageProcessor;

    public KafkaConsumerService(
        ILogger<KafkaConsumerService> logger,
        ConsumerConfig consumerConfig,
        RetryService retryService,
        IMessageChecker messageChecker,
        IMessageProcessor messageProcessor)
    {
        _logger = logger;
        _consumerConfig = consumerConfig;
        _retryService = retryService;
        _messageChecker = messageChecker;
        _messageProcessor = messageProcessor;
    }

    public async Task ConsumeAsync(string topic, CancellationToken cancellationToken)
    {
        using var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
        consumer.Subscribe(topic);
        _logger.LogInformation("Subscribed to topic: {Topic}", topic);

        while (!cancellationToken.IsCancellationRequested)
        {
            if (_retryService.CircuitStateIsOpen)
            {
                await Task.Delay(5000, cancellationToken);
                continue;
            }

            ConsumeResult<Ignore, string>? consumerResult = null;

            try
            {
                consumerResult = consumer.Consume(cancellationToken);
            }
            catch (ConsumeException e)
            {
                _logger.LogError("Consume error: {Reason}. Skipping commit.", e.Error.Reason);
                continue;
            }

            var message = consumerResult.Message;
            int retryCount = GetRetryCount(message.Headers);

            if (await _messageChecker.CanProcessMessageAsync(message, cancellationToken))
            {
                await _retryService.ExecuteWithRetryAsync(async ct =>
                {
                    await _messageProcessor.ProcessMessageAsync(message, ct);
                }, message, retryCount, cancellationToken);
            }

            CommitMessage(consumer, consumerResult);
        }
    }

    public void CommitMessage(IConsumer<Ignore, string> consumer, ConsumeResult<Ignore, string> consumerResult)
    {
        try
        {
            consumer.Commit(consumerResult);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to commit message: {Message}", ex.Message);
        }
    }

    private int GetRetryCount(Headers headers)
    {
        var header = headers.FirstOrDefault(h => h.Key == "retry-count");
        return header == null ? 0 : int.Parse(Encoding.UTF8.GetString(header.GetValueBytes()));
    }
}

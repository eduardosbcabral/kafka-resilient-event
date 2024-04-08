using KafkaResilientEvent.Settings;

namespace KafkaResilientEvent;

public class KafkaWorker : BackgroundService
{
    private readonly ILogger<KafkaWorker> _logger;
    private readonly KafkaConsumerService _kafkaConsumerService;
    private readonly AppSettings _appSettings;

    public KafkaWorker(
        ILogger<KafkaWorker> logger,
        KafkaConsumerService kafkaConsumerService,
        AppSettings appSettings)
    {
        _logger = logger;
        _kafkaConsumerService = kafkaConsumerService;
        _appSettings = appSettings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(() => _logger.LogDebug($"#1 KafkaWorker is stopping."));

        try
        {
            _logger.LogInformation("#2 KafkaWorker running at: {time}", DateTimeOffset.UtcNow);
            var mainTopic = _kafkaConsumerService.ConsumeAsync(_appSettings.KafkaSettings.Topic, stoppingToken);
            var retryTopic = _kafkaConsumerService.ConsumeAsync(_appSettings.KafkaSettings.RetryTopic, stoppingToken);

            await Task.WhenAll(mainTopic, retryTopic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "#3 KafkaWorker encountered an error.");
        }

        _logger.LogDebug("#4 KafkaWorker is stopping.");
    }
}

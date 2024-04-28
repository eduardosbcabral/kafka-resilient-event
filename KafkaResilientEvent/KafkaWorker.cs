using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Polly;

namespace KafkaResilientEvent;

internal class KafkaWorker : BackgroundService
{
    private readonly ILogger<KafkaWorker> _logger;

    private readonly IEnumerableOfKeyd<ConsumerHandlerContext> _consumerHandlersContexts;

    public KafkaWorker(
        ILogger<KafkaWorker> logger,
        IEnumerableOfKeyd<ConsumerHandlerContext> consumerHandlersContexts)
    {
        _logger = logger;
        _consumerHandlersContexts = consumerHandlersContexts;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(() => _logger.LogDebug($"#1 KafkaWorker is stopping."));

        try
        {
            _logger.LogInformation("#2 KafkaWorker running at: {time}", DateTimeOffset.UtcNow);

            var tasks = _consumerHandlersContexts.Select(x =>
                Task.Run(() => x.ConsumerHandler.ConsumeAsync(x.Topic, stoppingToken), stoppingToken)
            );

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "#3 KafkaWorker encountered an error.");
        }

        _logger.LogDebug("#4 KafkaWorker is stopping.");
    }
}

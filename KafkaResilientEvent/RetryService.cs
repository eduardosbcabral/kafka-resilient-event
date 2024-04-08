using Confluent.Kafka;

using KafkaResilientEvent.Settings;

using Polly;
using Polly.CircuitBreaker;
using Polly.Contrib.WaitAndRetry;

public class RetryService
{
    private readonly ILogger<RetryService> _logger;
    private readonly PublisherService _publisherService;
    private readonly AppSettings _appSettings;

    private AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
    public IAsyncPolicy RetryPolicy { get; private set; }

    public bool CircuitStateIsOpen => _circuitBreakerPolicy.CircuitState == CircuitState.Open;

    public RetryService(
        ILogger<RetryService> logger,
        PublisherService publisherService,
        AppSettings appSettings)
    {
        _logger = logger;
        _publisherService = publisherService;
        _appSettings = appSettings;

        var cbSettings = _appSettings.CircuitBreakerSettings;
        _circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: cbSettings.ExceptionsAllowedBeforeBreaking,
                durationOfBreak: TimeSpan.FromSeconds(cbSettings.DurationOfBreakInSeconds),
                onBreak: (ex, breakDelay) =>
                {
                    logger.LogWarning("Circuit breaker opened for {TotalSeconds} seconds due to: {Message}", breakDelay.TotalSeconds, ex.Message);
                },
                onReset: () => logger.LogInformation("Circuit breaker reset."),
                onHalfOpen: () => logger.LogInformation("Circuit breaker is half-open."));

        var jitter = Backoff.DecorrelatedJitterBackoffV2(
            medianFirstRetryDelay: TimeSpan.FromMilliseconds(_appSettings.RetrySettings.MedianFirstRetryDelayInMilliseconds),
            retryCount: _appSettings.RetrySettings.MaxRetryAttempts);

        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(jitter, onRetry: (exception, timespan, attempt, context) =>
            {
                _logger.LogWarning("Retrying due to: {Message}. Attempt: {attempt}", exception.Message, attempt);
            });

        var fallbackPolicy = Policy
            .Handle<Exception>()
            .FallbackAsync(async (context, cancellationToken) =>
            {
                var message = (Message<Ignore, string>)context["message"];
                var retryCount = (int)context["retryCount"];
                await HandleFailedProcessing(message, retryCount, cancellationToken);
            }, (exception, context) =>
            {
                var attempt = (int)context["retryCount"];
                _logger.LogWarning("{Message}. Attempt: {attempt}", exception.Message, attempt);
                return Task.CompletedTask;
            });

        RetryPolicy = fallbackPolicy.WrapAsync(_circuitBreakerPolicy).WrapAsync(retryPolicy);
    }

    public async Task ExecuteWithRetryAsync(Func<CancellationToken, Task> action, Message<Ignore, string> message, int retryCount, CancellationToken cancellationToken)
    {
        var context = new Context
        {
            ["message"] = message,
            ["retryCount"] = retryCount
        };

        await RetryPolicy.ExecuteAsync((ctx) => action(cancellationToken), context);
    }

    private async Task HandleFailedProcessing(Message<Ignore, string> message, int retryCount, CancellationToken cancellationToken)
    {
        if (retryCount >= _appSettings.RetrySettings.PublishRetryCount)
        {
            await _publisherService.PublishMessageAsync(_appSettings.KafkaSettings.DeadLetterTopic, message, retryCount, cancellationToken);
        }
        else
        {
            await _publisherService.PublishMessageAsync(_appSettings.KafkaSettings.RetryTopic, message, retryCount + 1, cancellationToken);
        }
    }
}

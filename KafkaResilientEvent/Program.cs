using Confluent.Kafka;

using KafkaResilientEvent;
using KafkaResilientEvent.Settings;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true);

IConfigurationRoot configuration = builder.Build();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var kafkaSettings = hostContext.Configuration.GetSection("KafkaSettings").Get<KafkaSettings>();
        var retrySettings = hostContext.Configuration.GetSection("RetrySettings").Get<RetrySettings>();
        var circuitBreakerSettings = hostContext.Configuration.GetSection("CircuitBreakerSettings").Get<CircuitBreakerSettings>();

        ArgumentNullException.ThrowIfNull(kafkaSettings);
        ArgumentNullException.ThrowIfNull(retrySettings);
        ArgumentNullException.ThrowIfNull(circuitBreakerSettings);

        // Configure seus serviços aqui
        services.AddHostedService<KafkaWorker>();
        services.AddSingleton<KafkaConsumerService>();

        var appSettings = new AppSettings
        {
            KafkaSettings = kafkaSettings,
            RetrySettings = retrySettings,
            CircuitBreakerSettings = circuitBreakerSettings
        };
        // Registro dos serviços
        services.AddSingleton(appSettings);

        services.AddSingleton<RetryService>();
        services.AddSingleton<PublisherService>();
        services.AddSingleton<KafkaConsumerService>();
        services.AddSingleton(new ConsumerConfig
        {
            BootstrapServers = kafkaSettings.BootstrapServers,
            GroupId = kafkaSettings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Latest,
            EnableAutoCommit = false
        });

        services.AddSingleton<IMessageChecker, SimpleMessageChecker>();
        services.AddSingleton<IMessageProcessor, SimpleMessageProcessor>();

        services.AddSingleton(_ => {
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = kafkaSettings.BootstrapServers,
                Acks = Acks.All,
            };
            return new ProducerBuilder<Null, string>(producerConfig)
                .SetValueSerializer(Serializers.Utf8)
                .Build();
        });
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddJsonConsole();
    })
    .Build();

var services = host.Services;

var appSettings = services.GetRequiredService<AppSettings>();
var logger = services.GetRequiredService<ILogger<Program>>();

PrintAppSettingsVariables(logger, appSettings);

await host.RunAsync();

void PrintAppSettingsVariables(ILogger<Program> logger, AppSettings appSettings)
{
    logger.LogInformation("AppSettings: {appSettings}", appSettings);
}
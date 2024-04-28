using KafkaResilientEvent.Sample;
using KafkaResilientEvent.Settings;

using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

var services = builder.Services;

var kafkaSettings = builder.Configuration.GetSection("KafkaSettings").Get<KafkaSettings>();
var retrySettings = builder.Configuration.GetSection("RetrySettings").Get<RetrySettings>();
var circuitBreakerSettings = builder.Configuration.GetSection("CircuitBreakerSettings").Get<CircuitBreakerSettings>();

ArgumentNullException.ThrowIfNull(kafkaSettings);
ArgumentNullException.ThrowIfNull(retrySettings);
ArgumentNullException.ThrowIfNull(circuitBreakerSettings);

builder.Services.AddSingleton(x => Options.Create(kafkaSettings));
builder.Services.AddSingleton(x => Options.Create(retrySettings));
builder.Services.AddSingleton(x => Options.Create(circuitBreakerSettings));

//services.AddHostedService<Worker>();

//services.AddKeyedTransient<IConsumer<KafkaMessageOne>, KafkaMessageOneConsumer>($"Consumer.{nameof(KafkaMessageOne)}");
//services.AddKeyedTransient<IConsumer<KafkaMessageTwo>, KafkaMessageTwoConsumer>($"Consumer.{nameof(KafkaMessageTwo)}");

//var consumerConfig = new ConsumerConfig
//{
//    BootstrapServers = kafkaSettings.BootstrapServers,
//    GroupId = kafkaSettings.GroupId,
//    AutoOffsetReset = AutoOffsetReset.Latest,
//    EnableAutoCommit = false
//};
//services.AddSingleton(consumerConfig);
//services.AddTransient(provider =>
//{
//    var handler = new ConsumerHandler(
//        provider.GetRequiredService<ILogger<ConsumerHandler>>(),
//        provider,
//        consumerConfig,
//        [typeof(KafkaMessageOne), typeof(KafkaMessageTwo)]
//    );

//    return handler;
//});

services.ConfigureKafka(x =>
{
    x.Host(kafkaSettings.BootstrapServers);

    x.ConfigureTopic(kafkaSettings.Topics[0].Topic, kafkaSettings.GroupId, x =>
    {
        x.ConfigureConsumer<KafkaMessageOne, KafkaMessageOneConsumer>();
        x.ConfigureConsumer<KafkaMessageTwo, KafkaMessageTwoConsumer>();
        x.ConfigureHandler();
    });
    x.ConfigureTopic(kafkaSettings.Topics[1].Topic, kafkaSettings.GroupId, x =>
    {
        x.ConfigureConsumer<KafkaMessageThree, KafkaMessageThreeConsumer>();
        x.ConfigureHandler();
    });
    x.ConfigureKafkaWorker();
});

var host = builder.Build();

host.Run();

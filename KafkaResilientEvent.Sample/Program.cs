using Confluent.Kafka;

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

services.ConfigureKafka(x =>
{
    x.Host(kafkaSettings.BootstrapServers);

    x.ConfigureTopic(kafkaSettings.Topics[0].Topic, kafkaSettings.GroupId, x =>
    {
        x.ConfigureEvent<Ignore, KafkaMessageOne>();
        x.ConfigureEvent<Ignore, KafkaMessageTwo>();
    });
    x.ConfigureTopic(kafkaSettings.Topics[1].Topic, kafkaSettings.GroupId, x =>
    {
        x.ConfigureEvent<Ignore, KafkaMessageThree>();
    });
});

services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

var host = builder.Build();

host.Run();

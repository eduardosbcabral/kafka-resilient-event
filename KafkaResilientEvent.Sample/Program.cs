using KafkaResilientEvent;
using KafkaResilientEvent.Sample;
using KafkaResilientEvent.Settings;

using MassTransit;
using MassTransit.Middleware;

var builder = Host.CreateApplicationBuilder(args);

var kafkaSettings = builder.Configuration.GetSection("KafkaSettings").Get<KafkaSettings>();
var retrySettings = builder.Configuration.GetSection("RetrySettings").Get<RetrySettings>();
var circuitBreakerSettings = builder.Configuration.GetSection("CircuitBreakerSettings").Get<CircuitBreakerSettings>();

ArgumentNullException.ThrowIfNull(kafkaSettings);
ArgumentNullException.ThrowIfNull(retrySettings);
ArgumentNullException.ThrowIfNull(circuitBreakerSettings);

builder.Services.AddTransient<IKafkaMessageGatekeeper<KafkaMessage>, KafkaMessageGatekeeper>();
builder.Services.AddTransient<IKafkaMessageProcessor<KafkaMessage>, KafkaMessageProcessor>();

builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory();

    x.AddRider(rider =>
    {
        rider.AddConsumer<KafkaMessageConsumer<KafkaMessage>>();

        rider.UsingKafka((context, k) =>
        {
            k.Host(kafkaSettings.BootstrapServers);

            k.TopicEndpoint<KafkaMessage>(kafkaSettings.Topic, kafkaSettings.GroupId, e =>
            {
                e.ConfigureConsumer<KafkaMessageConsumer<KafkaMessage>>(context);
                e.UseConsumeFilter<DiscardMessageFilter>(context);

                if (retrySettings.UseRetry)
                {
                    e.UseMessageRetry(r =>
                    {
                        r.Exponential(
                            retrySettings.Limit,
                            TimeSpan.FromMilliseconds(retrySettings.MinIntervalInMilliseconds),
                            TimeSpan.FromMilliseconds(retrySettings.MaxIntervalInMilliseconds),
                            TimeSpan.FromMilliseconds(retrySettings.IntervalDeltaInMilliseconds)
                        );
                    });

                    if (retrySettings.UseRedelivery)
                    {
                        e.UseDelayedRedelivery(r =>
                        {
                            r.Intervals(retrySettings.RedeliveryIntervalsInMinutes.Select(x => TimeSpan.FromMinutes(x)).ToArray());
                        });
                    }
                }
            });
        });
    });
});

var host = builder.Build();
host.Run();

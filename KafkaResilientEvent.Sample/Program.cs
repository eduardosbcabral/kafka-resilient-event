using KafkaResilientEvent.Sample;
using KafkaResilientEvent.Settings;

using MassTransit;

var builder = Host.CreateApplicationBuilder(args);

var kafkaSettings = builder.Configuration.GetSection("KafkaSettings").Get<KafkaSettings>();
var retrySettings = builder.Configuration.GetSection("RetrySettings").Get<RetrySettings>();
var circuitBreakerSettings = builder.Configuration.GetSection("CircuitBreakerSettings").Get<CircuitBreakerSettings>();

ArgumentNullException.ThrowIfNull(kafkaSettings);
ArgumentNullException.ThrowIfNull(retrySettings);
ArgumentNullException.ThrowIfNull(circuitBreakerSettings);

builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory();

    x.AddRider(rider =>
    {
        rider.AddConsumer<KafkaMessageConsumer>();

        rider.UsingKafka((context, k) =>
        {
            k.Host(kafkaSettings.BootstrapServers);

            k.TopicEndpoint<KafkaMessage>(kafkaSettings.Topic, kafkaSettings.GroupId, e =>
            {
                e.ConfigureConsumer<KafkaMessageConsumer>(context);
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
                        // Não é possível implementar nativamente o redelivery com o Kafka na lib do Masstransit
                        e.UseDelayedRedelivery(r =>
                        {
                            //r.Intervals(retrySettings.RedeliveryIntervalsInMinutes.Select(x => TimeSpan.FromMinutes(x)).ToArray());
                            r.Intervals([TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30)]);
                        });
                    }
                }
            });
        });
    });
});

var host = builder.Build();
host.Run();

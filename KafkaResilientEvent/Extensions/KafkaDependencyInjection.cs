using Confluent.Kafka;

using KafkaResilientEvent;

using MediatR;

using Microsoft.Extensions.Logging;

using System.ComponentModel.DataAnnotations;

namespace Microsoft.Extensions.DependencyInjection;

public static class KafkaDependencyInjection
{
    public static IServiceCollection ConfigureKafka(this IServiceCollection services, Action<IConfigureKafkaBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(services);

        var builder = new ConfigureKafkaBuilder(services);
        configure(builder);

        var handlers = services.Where(sd => sd.ServiceType == typeof(ConsumerHandlerContext));
        services.AddTransient<IKeyEnumerable<ConsumerHandlerContext>>(p =>
        {
            var s = handlers.Select(x =>
            {
                return p.GetRequiredKeyedService<ConsumerHandlerContext>(x.ServiceKey);
            });
            return new KeyEnumerable<ConsumerHandlerContext>(s);
        });

        services.AddHostedService<KafkaWorker>();

        return services;
    }
}

class ConfigureKafkaBuilder(IServiceCollection services) : IConfigureKafkaBuilder
{
    [Required]
    public static string BootstrapServers { get; private set; } = string.Empty;

    public void Host(string bootstrapServers)
    {
        BootstrapServers = bootstrapServers;
    }

    public void ConfigureTopic(string topic, string groupId, Action<IConfigureTopicBuilder> configure)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = BootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Latest,
            EnableAutoCommit = false
        };

        var builder = new ConfigureTopicBuilder(services, topic, consumerConfig);
        configure(builder);

        builder.ConfigureConsumer();
    }
}

public interface IConfigureKafkaBuilder
{
    void Host(string bootstrapServers);
    void ConfigureTopic(string topic, string groupId, Action<IConfigureTopicBuilder> configure);
}

class ConfigureTopicBuilder(IServiceCollection services, string topic, ConsumerConfig consumerConfig) : IConfigureTopicBuilder
{
    private readonly Dictionary<string, KeyValuePair<Type, Type>> _eventTypes = [];

    public void ConfigureEvent<TKey, TValue>()
    {
        var keyType = typeof(TKey);
        var valueType = typeof(TValue);
        _eventTypes.Add(valueType.Name, new(keyType, valueType));
    }

    internal void ConfigureConsumer()
    {
        services.AddKeyedSingleton($"event-types.{topic}", _eventTypes);
        services.AddKeyedTransient($"consumer-handler.{topic}", (provider, _) =>
        {
            var handler = new ConsumerHandler(
                provider.GetRequiredService<ILogger<ConsumerHandler>>(),
                provider.GetRequiredService<IMediator>(),
                consumerConfig,
                provider.GetRequiredKeyedService<Dictionary<string, KeyValuePair<Type, Type>>>($"event-types.{topic}")
            );
            return new ConsumerHandlerContext(handler, topic);
        });
    }
}

public interface IConfigureTopicBuilder
{
    public void ConfigureEvent<TKey, TValue>();
}

class KeyEnumerable<T>(IEnumerable<T> values) : List<T>(values), IKeyEnumerable<T>
{
}

interface IKeyEnumerable<T> : IEnumerable<T>
{ }
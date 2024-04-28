using Confluent.Kafka;

using KafkaResilientEvent;
using KafkaResilientEvent.Settings;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.ComponentModel.DataAnnotations;

namespace Microsoft.Extensions.DependencyInjection;

public static class KafkaDependencyInjection
{
    public static IServiceCollection ConfigureKafka(this IServiceCollection services, Action<IConfigureKafkaBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(services);

        var builder = new ConfigureKafkaBuilder(services);
        configure(builder);

        return services;
    }
}

class ConfigureKafkaBuilder(IServiceCollection services) : IConfigureKafkaBuilder
{
    [Required]
    public static string BootstrapServers { get; private set; } = string.Empty;
    public static List<ConfigureTopicBuilder> ConfigureTopicBuilders { get; private set; } = [];

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
        services.AddKeyedSingleton(consumerConfig, groupId);

        var builder = new ConfigureTopicBuilder(services, topic, consumerConfig);
        configure(builder);
        ConfigureTopicBuilders.Add(builder);
    }

    public void ConfigureKafkaWorker()
    {
        RegisterKeyedEnumerable<string, ConsumerHandlerContext>(services);

        services.AddHostedService<KafkaWorker>();
    }

    void RegisterKeyedEnumerable<TKey, T>(IServiceCollection serviceCollection)
    {
        var keys = serviceCollection
            .Where(sd => sd.IsKeyedService && sd.ServiceType == typeof(T))
            .Select(d => d.ServiceKey)
            .Select(k => (TKey)k)
            .ToList();

        var a = serviceCollection
            .Where(sd => sd.IsKeyedService && sd.ServiceType == typeof(T))
            .ToList();

        serviceCollection.AddTransient<IEnumerableOfKeyd<T>>(p =>
        {
            var values = keys.Select(k => p.GetRequiredKeyedService<T>(k));
            return new EnumerableOfKeyd<T>(values);
        });
    }
}

public interface IConfigureKafkaBuilder
{
    void Host(string bootstrapServers);
    void ConfigureTopic(string topic, string groupId, Action<IConfigureTopicBuilder> configure);
    void ConfigureKafkaWorker();
}

class ConfigureTopicBuilder(IServiceCollection services, string topic, ConsumerConfig consumerConfig) : IConfigureTopicBuilder
{
    private IDictionary<string, Type> _eventTypes = new Dictionary<string, Type>();
    public string Topic = topic;

    public void ConfigureConsumer<TMessage, TConsumer>()
        where TMessage : IMessage
        where TConsumer : IConsumer<TMessage>
    {
        var messageType = typeof(TMessage);
        _eventTypes.Add(messageType.Name, messageType);
        services.AddKeyedTransient(typeof(IConsumer<TMessage>), $"Consumer.{messageType.Name}", typeof(TConsumer));
    }

    public void ConfigureHandler()
    {
        services.AddKeyedTransient($"ConsumerHandlerContext.{Guid.NewGuid()}", (provider, _) =>
        {
            var handler = new ConsumerHandler(
                provider.GetRequiredService<ILogger<ConsumerHandler>>(),
                provider,
                consumerConfig,
                _eventTypes
            );
            return new ConsumerHandlerContext(handler, Topic);
        });
    }
}

public interface IConfigureTopicBuilder
{
    void ConfigureConsumer<TMessage, TConsumer>() 
        where TMessage : IMessage
        where TConsumer : IConsumer<TMessage>;

    /// <summary>
    /// Call this method after configurating all consumers.
    /// </summary>
    void ConfigureHandler();
}

class EnumerableOfKeyd<T> : List<T>, IEnumerableOfKeyd<T>
{
    public EnumerableOfKeyd(IEnumerable<T> values) : base(values)
    {
    }
}

interface IEnumerableOfKeyd<T> : IEnumerable<T> 
{ }
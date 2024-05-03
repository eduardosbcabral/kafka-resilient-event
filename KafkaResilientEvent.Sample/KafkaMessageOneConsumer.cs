using Confluent.Kafka;

namespace KafkaResilientEvent.Sample;

public class KafkaMessageOneConsumer(ILogger<KafkaMessageOneConsumer> logger) : IConsumer<Ignore, KafkaMessageOne>
{
    public Task Handle(ConsumeContext<Ignore, KafkaMessageOne> request, CancellationToken cancellationToken)
    {
        logger.LogInformation("KafkaMessageOneConsumer");

        var payload = request.Value;

        if (payload.Text.Contains("error"))
        {
            logger.LogError("KafkaMessageOneConsumer");
            throw new Exception("KafkaMessageOneConsumer");
        }

        return Task.CompletedTask;
    }
}

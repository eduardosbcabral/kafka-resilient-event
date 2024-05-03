using Confluent.Kafka;

namespace KafkaResilientEvent.Sample;

public class KafkaMessageTwoConsumer(ILogger<KafkaMessageTwoConsumer> logger) : IConsumer<Ignore, KafkaMessageTwo>
{
    public Task Handle(ConsumeContext<Ignore, KafkaMessageTwo> request, CancellationToken cancellationToken)
    {
        logger.LogInformation("KafkaMessageTwoConsumer");

        var payload = request.Value;

        if (payload.Text.Contains("error"))
        {
            logger.LogError("KafkaMessageTwoConsumer");
            throw new Exception("KafkaMessageTwoConsumer");
        }

        return Task.CompletedTask;
    }
}

using Confluent.Kafka;

namespace KafkaResilientEvent.Sample;

public class KafkaMessageThreeConsumer(ILogger<KafkaMessageThreeConsumer> logger) : IConsumer<Ignore, KafkaMessageThree>
{
    public Task Handle(ConsumeContext<Ignore, KafkaMessageThree> request, CancellationToken cancellationToken)
    {
        logger.LogInformation("KafkaMessageThreeConsumer");

        var payload = request.Value;

        if (payload.Text.Contains("error"))
        {
            logger.LogError("KafkaMessageThreeConsumer");
            throw new Exception("KafkaMessageThreeConsumer");
        }

        return Task.CompletedTask;
    }
}

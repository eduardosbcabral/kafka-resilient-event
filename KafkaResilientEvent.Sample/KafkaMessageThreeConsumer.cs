namespace KafkaResilientEvent.Sample;

public class KafkaMessageThreeConsumer(ILogger<KafkaMessageThreeConsumer> logger) : IConsumer<KafkaMessageThree>
{
    public Task Consume(ConsumeContext<KafkaMessageThree> context, CancellationToken cancellationToken)
    {
        logger.LogInformation("KafkaMessageThreeConsumer");

        var payload = context.Message;

        if (payload.Text.Contains("error"))
        {
            logger.LogError("KafkaMessageThreeConsumer");
            throw new Exception("KafkaMessageThreeConsumer");
        }

        return Task.CompletedTask;
    }
}

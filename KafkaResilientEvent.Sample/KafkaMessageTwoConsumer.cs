namespace KafkaResilientEvent.Sample;

public class KafkaMessageTwoConsumer(ILogger<KafkaMessageTwoConsumer> logger) : IConsumer<KafkaMessageTwo>
{
    public Task Consume(ConsumeContext<KafkaMessageTwo> context, CancellationToken cancellationToken)
    {
        logger.LogInformation("KafkaMessageTwoConsumer");

        var payload = context.Message;

        if (payload.Text.Contains("error"))
        {
            logger.LogError("KafkaMessageTwoConsumer");
            throw new Exception("KafkaMessageTwoConsumer");
        }

        return Task.CompletedTask;
    }
}

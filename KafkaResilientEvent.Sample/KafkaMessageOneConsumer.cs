namespace KafkaResilientEvent.Sample;

public class KafkaMessageOneConsumer(ILogger<KafkaMessageOneConsumer> logger) : IConsumer<KafkaMessageOne>
{
    public Task Consume(ConsumeContext<KafkaMessageOne> context, CancellationToken cancellationToken)
    {
        logger.LogInformation("KafkaMessageOneConsumer");

        var payload = context.Message;

        if (payload.Text.Contains("error"))
        {
            logger.LogError("KafkaMessageOneConsumer");
            throw new Exception("KafkaMessageOneConsumer");
        }

        return Task.CompletedTask;
    }
}

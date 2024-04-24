using MassTransit;

namespace KafkaResilientEvent.Sample;

internal class KafkaMessageProcessor(ILogger<KafkaMessageProcessor> logger) : IKafkaMessageProcessor<KafkaMessage>
{
    public Task Consume(ConsumeContext<KafkaMessage> context)
    {
        logger.LogInformation("KafkaMessageProcessor");

        var payload = context.Message;

        if (payload.Text.Contains("error"))
        {
            logger.LogError("KafkaMessageProcessor");
            throw new Exception("KafkaMessageProcessor");
        }

        return Task.CompletedTask;
    }
}

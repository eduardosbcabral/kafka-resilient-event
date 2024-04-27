using MassTransit;

namespace KafkaResilientEvent.Sample;

internal class KafkaMessageConsumer(ILogger<KafkaMessageConsumer> logger) : IConsumer<KafkaMessage>
{
    public Task Consume(ConsumeContext<KafkaMessage> context)
    {
        var retry = context.GetRetryAttempt();
        logger.LogInformation("KafkaMessageConsumer");

        var payload = context.Message;

        if (payload.Text.Contains("error"))
        {
            logger.LogError("KafkaMessageConsumer");
            throw new Exception("KafkaMessageConsumer");
        }

        return Task.CompletedTask;
    }
}

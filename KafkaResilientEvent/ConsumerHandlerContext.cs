namespace KafkaResilientEvent;

public record ConsumerHandlerContext(ConsumerHandler ConsumerHandler, string Topic)
{
    public Task Run(CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            return ConsumerHandler.ConsumeAsync(Topic, cancellationToken);
        }, cancellationToken);
    }
}

namespace KafkaResilientEvent;

public interface IConsumer<TMessage> where TMessage : IMessage
{
    public Task Consume(ConsumeContext<TMessage> context, CancellationToken cancellationToken);
}

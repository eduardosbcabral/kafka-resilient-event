namespace KafkaResilientEvent;

public interface IDiscardMessageFilter<TMessage> where TMessage : IMessage
{
    Task Execute(ConsumeContext<TMessage> context);
}

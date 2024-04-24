using MassTransit;

namespace KafkaResilientEvent;

public interface IKafkaMessageProcessor<TMessage> where TMessage : class, IKafkaMessage
{
    public Task Consume(ConsumeContext<TMessage> context);
}

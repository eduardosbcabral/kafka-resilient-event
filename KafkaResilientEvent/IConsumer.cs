using MediatR;

namespace KafkaResilientEvent;

public interface IConsumer<TKey, TValue> : IRequestHandler<ConsumeContext<TKey, TValue>>
{
}

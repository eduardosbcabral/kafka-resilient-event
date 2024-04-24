using MassTransit;

namespace KafkaResilientEvent;

public class KafkaMessageConsumer<TMessage>(
    IKafkaMessageProcessor<TMessage> messageProcessor,
    IKafkaMessageGatekeeper<TMessage>? messageGatekeeper = null) : IConsumer<TMessage> where TMessage : class, IKafkaMessage
{
    public async Task Consume(ConsumeContext<TMessage> context)
    {
        var shouldProcess = messageGatekeeper is null || await messageGatekeeper.ShouldProcessMessage(context).ConfigureAwait(false);
        if (!shouldProcess)
        {
            return;
        }

        await messageProcessor.Consume(context).ConfigureAwait(false);
    }
}

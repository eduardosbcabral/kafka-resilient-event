using MassTransit;

using Microsoft.Extensions.Logging;

namespace KafkaResilientEvent;

internal class DelayedMessageRedeliveryFilter<TMessage>(ILogger<DelayedMessageRedeliveryFilter<TMessage>> logger) : IFilter<SendContext<TMessage>> where TMessage : class
{
    public async Task Send(SendContext<TMessage> context, IPipe<SendContext<TMessage>> next)
    {

        await next.Send(context).ConfigureAwait(false);
    }

    public void Probe(ProbeContext context) { }
}

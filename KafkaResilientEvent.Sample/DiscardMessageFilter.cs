using MassTransit;

namespace KafkaResilientEvent.Sample;

internal class DiscardMessageFilter : IFilter<ConsumeContext<KafkaMessage>>
{
    public async Task Send(ConsumeContext<KafkaMessage> context, IPipe<ConsumeContext<KafkaMessage>> next)
    {
        var payload = context.Message;
        if (payload.Text.Contains("process"))
        {
            return;
        }

        await next.Send(context).ConfigureAwait(false);
    }

    public void Probe(ProbeContext context) { }
}

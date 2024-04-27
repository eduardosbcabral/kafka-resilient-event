using MassTransit;

namespace KafkaResilientEvent.Sample;

internal class DiscardMessageFilter(ILogger<DiscardMessageFilter> logger) : IFilter<ConsumeContext<KafkaMessage>>
{
    public async Task Send(ConsumeContext<KafkaMessage> context, IPipe<ConsumeContext<KafkaMessage>> next)
    {
        if (context.GetRetryAttempt() == 0)
        {
            logger.LogInformation("DiscardMessageFilter");

            var payload = context.Message;
            if (!payload.Text.Contains("process"))
            {
                return;
            }

        }

        await next.Send(context).ConfigureAwait(false);
    }

    public void Probe(ProbeContext context) 
    {
        context.CreateFilterScope("discardMessage");
    }
}

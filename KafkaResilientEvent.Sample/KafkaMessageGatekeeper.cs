using MassTransit;

namespace KafkaResilientEvent.Sample;

internal class KafkaMessageGatekeeper(ILogger<KafkaMessageGatekeeper> logger)
{
    public Task<bool> ShouldProcessMessage(ConsumeContext<KafkaMessage> context)
    {
        logger.LogInformation("KafkaMessageGatekeeper");
        var payload = context.Message;
        return Task.FromResult(payload.Text.Contains("process"));
    }
}

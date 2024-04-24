using MassTransit;

namespace KafkaResilientEvent;

public interface IKafkaMessageGatekeeper<KM> where KM : class, IKafkaMessage
{
    public Task<bool> ShouldProcessMessage(ConsumeContext<KM> context);
}

namespace KafkaResilientEvent;

public record ConsumerHandlerContext(ConsumerHandler ConsumerHandler, string Topic)
{
}

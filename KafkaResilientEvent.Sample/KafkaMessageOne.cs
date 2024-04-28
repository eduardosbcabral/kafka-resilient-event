namespace KafkaResilientEvent.Sample;

public record KafkaMessageOne : IMessage
{
    public required string Text { get; init; }
}

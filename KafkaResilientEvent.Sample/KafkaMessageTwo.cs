namespace KafkaResilientEvent.Sample;

public record KafkaMessageTwo : IMessage
{
    public required string Text { get; init; }
}
namespace KafkaResilientEvent.Sample;

public record KafkaMessageThree : IMessage
{
    public required string Text { get; init; }
}
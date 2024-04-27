namespace KafkaResilientEvent.Sample;

record KafkaMessage
{
    public required string Text { get; init; }
}

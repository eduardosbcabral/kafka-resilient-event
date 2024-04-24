namespace KafkaResilientEvent.Sample;

record KafkaMessage : IKafkaMessage
{
    public required string Text { get; init; }
}

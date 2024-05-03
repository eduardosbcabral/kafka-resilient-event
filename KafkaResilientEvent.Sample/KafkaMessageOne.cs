namespace KafkaResilientEvent.Sample;

public record KafkaMessageOne : IMessage
{
    public string Text { get; set; }
}

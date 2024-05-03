namespace KafkaResilientEvent.Sample;

public record KafkaMessageTwo : IMessage
{
    public string Text { get; set; }
}
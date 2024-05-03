namespace KafkaResilientEvent.Sample;

public record KafkaMessageThree : IMessage
{
    public string Text { get; set; }
}
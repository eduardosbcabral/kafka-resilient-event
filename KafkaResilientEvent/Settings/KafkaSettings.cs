namespace KafkaResilientEvent.Settings;

public class KafkaSettings
{
    public string BootstrapServers { get; set; }
    public string GroupId { get; set; }
    public string Topic { get; set; }
    public string RetryTopic { get; set; }
    public string DeadLetterTopic { get; set; }
}

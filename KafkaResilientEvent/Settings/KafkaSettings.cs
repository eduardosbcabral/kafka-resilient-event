namespace KafkaResilientEvent.Settings;

public record KafkaSettings(string BootstrapServers, string GroupId, List<TopicSettings> Topics);
public record TopicSettings(string Topic, string Retry, string DeadLetter);
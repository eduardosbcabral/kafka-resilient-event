namespace KafkaResilientEvent.Settings;

public class RetrySettings
{
    public int MedianFirstRetryDelayInMilliseconds { get; set; } = 500;
    public int MaxRetryAttempts { get; set; } = 5;
    public int PublishRetryCount { get; set; } = 3;
}

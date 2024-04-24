namespace KafkaResilientEvent.Settings;

public class RetrySettings
{
    public bool UseRetry { get; set; } = true;
    public int Limit { get; set; } = 5;
    public double MinIntervalInMilliseconds { get; set; } = 100;
    public double MaxIntervalInMilliseconds { get; set; } = 500;
    public double IntervalDeltaInMilliseconds { get; set; } = 350;

    public bool UseRedelivery { get; set; } = true;
    public double[] RedeliveryIntervalsInMinutes { get; set; } = [5, 15, 30];
}

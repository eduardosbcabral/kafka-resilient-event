namespace KafkaResilientEvent.Settings;

public class CircuitBreakerSettings
{
    public int ExceptionsAllowedBeforeBreaking { get; set; }
    public int DurationOfBreakInSeconds { get; set; }
}

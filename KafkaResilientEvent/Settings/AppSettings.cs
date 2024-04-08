using System.Text.Json;

namespace KafkaResilientEvent.Settings;

public class AppSettings
{
    public KafkaSettings KafkaSettings { get; set; }
    public RetrySettings RetrySettings { get; set; }
    public CircuitBreakerSettings CircuitBreakerSettings { get; set; }

    public override string? ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
